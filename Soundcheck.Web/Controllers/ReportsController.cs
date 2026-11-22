using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;

namespace Soundcheck.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController(ApplicationDbContext databaseContext) : Controller
{
    public async Task<IActionResult> Revenue()
    {
        var reportRows = await databaseContext.ConsultantRevenueReport
            .AsNoTracking()
            .OrderBy(row => row.ConsultantName)
            .ThenBy(row => row.CategoryName)
            .ToListAsync();

        return View(reportRows);
    }
}
