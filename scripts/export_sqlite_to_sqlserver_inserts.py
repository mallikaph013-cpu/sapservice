import sqlite3
from pathlib import Path
from collections import defaultdict, deque

ROOT = Path(__file__).resolve().parents[1]
SQLITE_DB = ROOT / "myapp.db"
OUTPUT_SQL = ROOT / "sqlite-to-sqlserver-inserts.sql"

EXCLUDED_TABLES = {
    "sqlite_sequence",
    "__EFMigrationsHistory",
}


def quote_ident(name: str) -> str:
    return "[" + name.replace("]", "]]" ) + "]"


def to_sqlserver_literal(value):
    if value is None:
        return "NULL"
    if isinstance(value, (int, float)):
        return str(value)
    if isinstance(value, (bytes, bytearray)):
        return "0x" + bytes(value).hex().upper()

    text = str(value)
    text = text.replace("'", "''")
    return f"N'{text}'"


def get_tables(conn: sqlite3.Connection):
    rows = conn.execute(
        """
        SELECT name
        FROM sqlite_master
        WHERE type='table'
        ORDER BY name
        """
    ).fetchall()
    tables = [r[0] for r in rows if r[0] not in EXCLUDED_TABLES]
    return tables


def get_columns(conn: sqlite3.Connection, table: str):
    info = conn.execute(f"PRAGMA table_info('{table}')").fetchall()
    # cid, name, type, notnull, dflt_value, pk
    cols = [row[1] for row in info]
    pk_cols = [row for row in info if row[5] > 0]
    return info, cols, pk_cols


def has_identity_like_pk(pk_cols):
    if len(pk_cols) != 1:
        return False
    col = pk_cols[0]
    col_type = (col[2] or "").upper()
    # SQLite INTEGER PK maps commonly to SQL Server identity in this project
    return "INT" in col_type


def get_fk_graph(conn: sqlite3.Connection, tables):
    deps = defaultdict(set)
    reverse = defaultdict(set)

    table_set = set(tables)
    for t in tables:
        fk_rows = conn.execute(f"PRAGMA foreign_key_list('{t}')").fetchall()
        # id, seq, table, from, to, on_update, on_delete, match
        for fk in fk_rows:
            parent = fk[2]
            if parent in table_set and parent != t:
                deps[t].add(parent)
                reverse[parent].add(t)

    return deps, reverse


def topo_sort_tables(tables, deps, reverse):
    indegree = {t: len(deps[t]) for t in tables}
    q = deque(sorted([t for t in tables if indegree[t] == 0]))
    ordered = []

    while q:
        t = q.popleft()
        ordered.append(t)
        for child in sorted(reverse[t]):
            indegree[child] -= 1
            if indegree[child] == 0:
                q.append(child)

    if len(ordered) != len(tables):
        # fallback: append missing tables in alphabetical order
        missing = sorted(set(tables) - set(ordered))
        ordered.extend(missing)

    return ordered


def main():
    if not SQLITE_DB.exists():
        raise FileNotFoundError(f"SQLite db not found: {SQLITE_DB}")

    conn = sqlite3.connect(str(SQLITE_DB))
    conn.row_factory = sqlite3.Row

    tables = get_tables(conn)
    deps, reverse = get_fk_graph(conn, tables)
    ordered_tables = topo_sort_tables(tables, deps, reverse)

    lines = []
    lines.append("SET NOCOUNT ON;")
    lines.append("BEGIN TRANSACTION;")
    lines.append("")

    total_rows = 0

    for table in ordered_tables:
        info, cols, pk_cols = get_columns(conn, table)
        if not cols:
            continue

        rows = conn.execute(f"SELECT * FROM {quote_ident(table)}").fetchall()
        if not rows:
            continue

        total_rows += len(rows)

        lines.append(f"-- Table: {table} ({len(rows)} rows)")

        identity_like = has_identity_like_pk(pk_cols)
        if identity_like:
            lines.append(f"SET IDENTITY_INSERT {quote_ident(table)} ON;")

        col_list = ", ".join(quote_ident(c) for c in cols)
        for row in rows:
            values = ", ".join(to_sqlserver_literal(row[c]) for c in cols)
            lines.append(
                f"INSERT INTO {quote_ident(table)} ({col_list}) VALUES ({values});"
            )

        if identity_like:
            lines.append(f"SET IDENTITY_INSERT {quote_ident(table)} OFF;")

        lines.append("")

    lines.append("COMMIT TRANSACTION;")
    lines.append("")

    OUTPUT_SQL.write_text("\n".join(lines), encoding="utf-8")

    print(f"Generated: {OUTPUT_SQL}")
    print(f"Tables: {len(ordered_tables)}")
    print(f"Rows: {total_rows}")


if __name__ == "__main__":
    main()
