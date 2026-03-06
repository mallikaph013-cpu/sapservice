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

        public IActionResult Index(string? action, string? entity, string? performedBy, DateTime? fromDate, DateTime? toDate)
        {
            return NotFound();
        }
    }
}
