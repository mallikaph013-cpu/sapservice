import sqlite3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SQLITE_DB = ROOT / "myapp.db"
OUTPUT_SQL = ROOT / "sqlserver-insert-users-from-sqlite.sql"

TABLES = [
    "AspNetRoles",
    "AspNetUsers",
    "AspNetUserRoles",
    "AspNetUserClaims",
    "AspNetRoleClaims",
    "AspNetUserLogins",
    "AspNetUserTokens",
]


def qname(name: str) -> str:
    return f"[{name}]"


def sql_string(value):
    if value is None:
        return "NULL"
    if isinstance(value, (int, float)):
        return str(value)
    text = str(value).replace("'", "''")
    return f"N'{text}'"


def load_table(cursor, table_name: str):
    cursor.execute(f"PRAGMA table_info({table_name})")
    cols = [row[1] for row in cursor.fetchall()]
    cursor.execute(f"SELECT * FROM {table_name}")
    rows = cursor.fetchall()
    return cols, rows


def build_insert(table_name: str, cols, rows):
    if not rows:
        return [f"PRINT N'[INFO] No rows in {table_name}';"]

    col_list = ", ".join(qname(c) for c in cols)
    statements = [f"PRINT N'[INFO] Inserting {len(rows)} row(s) into {table_name}';"]

    for row in rows:
        values = ", ".join(sql_string(v) for v in row)
        statements.append(f"INSERT INTO {qname(table_name)} ({col_list}) VALUES ({values});")

    return statements


def main():
    if not SQLITE_DB.exists():
        raise FileNotFoundError(f"SQLite DB not found: {SQLITE_DB}")

    conn = sqlite3.connect(SQLITE_DB)
    cur = conn.cursor()

    lines = [
        "SET NOCOUNT ON;",
        "BEGIN TRY",
        "    BEGIN TRANSACTION;",
        "",
        "    -- Generated from myapp.db (SQLite)",
        "    -- User-related seed/migration data for SQL Server",
        "",
        "    DELETE FROM [AspNetUserTokens];",
        "    DELETE FROM [AspNetUserLogins];",
        "    DELETE FROM [AspNetUserClaims];",
        "    DELETE FROM [AspNetRoleClaims];",
        "    DELETE FROM [AspNetUserRoles];",
        "    DELETE FROM [AspNetUsers];",
        "    DELETE FROM [AspNetRoles];",
        "",
    ]

    for table in TABLES:
        cols, rows = load_table(cur, table)
        insert_lines = build_insert(table, cols, rows)
        lines.extend(f"    {line}" for line in insert_lines)
        lines.append("")

    lines.extend([
        "    COMMIT TRANSACTION;",
        "    PRINT N'[SUCCESS] User data import completed.';",
        "END TRY",
        "BEGIN CATCH",
        "    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;",
        "    THROW;",
        "END CATCH",
        "",
    ])

    OUTPUT_SQL.write_text("\n".join(lines), encoding="utf-8")
    conn.close()
    print(f"Generated: {OUTPUT_SQL}")


if __name__ == "__main__":
    main()
