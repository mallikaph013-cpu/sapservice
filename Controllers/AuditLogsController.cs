using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize(Roles = "IT")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? action, string? entity, string? performedBy, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(entity))
            {
                query = query.Where(a => a.EntityName.Contains(entity));
            }

            if (!string.IsNullOrWhiteSpace(performedBy))
            {
                query = query.Where(a => a.PerformedBy != null && a.PerformedBy.Contains(performedBy));
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(a => a.PerformedAt >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1);
                query = query.Where(a => a.PerformedAt < to);
            }

            ViewBag.Action = action;
            ViewBag.Entity = entity;
            ViewBag.PerformedBy = performedBy;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            ViewBag.ActionOptions = await _context.AuditLogs
                .AsNoTracking()
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var logs = await query
                .OrderByDescending(a => a.PerformedAt)
                .Take(1000)
                .ToListAsync();

            return View(logs);
        }
    }
}
