using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Food.MVC.Data;
using Food.Domain.Models;
using Food.MVC.ViewModels;

namespace Food.MVC.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? town, RiskRating? riskRating)
    {
        _logger.LogInformation("Dashboard accessed by user: {UserName}, Filters: Town={Town}, RiskRating={RiskRating}", 
            User.Identity?.Name, town ?? "None", riskRating?.ToString() ?? "None");

        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var premisesQuery = _context.Premises.AsQueryable();

        if (!string.IsNullOrEmpty(town))
        {
            premisesQuery = premisesQuery.Where(p => p.Town == town);
            _logger.LogInformation("Applied town filter: {Town}", town);
        }

        if (riskRating.HasValue)
        {
            premisesQuery = premisesQuery.Where(p => p.RiskRating == riskRating.Value);
            _logger.LogInformation("Applied risk rating filter: {RiskRating}", riskRating.Value);
        }

        var relevantPremisesIds = await premisesQuery.Select(p => p.Id).ToListAsync();

        var inspectionsThisMonth = await _context.Inspections
            .Where(i => relevantPremisesIds.Contains(i.PremisesId) && 
                        i.InspectionDate >= startOfMonth && 
                        i.InspectionDate < endOfMonth)
            .CountAsync();

        var failedInspectionsThisMonth = await _context.Inspections
            .Where(i => relevantPremisesIds.Contains(i.PremisesId) && 
                        i.InspectionDate >= startOfMonth && 
                        i.InspectionDate < endOfMonth &&
                        i.Outcome == InspectionOutcome.Fail)
            .CountAsync();

        var overdueFollowUps = await _context.FollowUps
            .Include(f => f.Inspection)
            .Where(f => relevantPremisesIds.Contains(f.Inspection!.PremisesId) &&
                        f.Status == FollowUpStatus.Open && 
                        f.DueDate < DateTime.Now)
            .CountAsync();

        var availableTowns = await _context.Premises
            .Select(p => p.Town)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            InspectionsThisMonth = inspectionsThisMonth,
            FailedInspectionsThisMonth = failedInspectionsThisMonth,
            OverdueFollowUps = overdueFollowUps,
            FilterTown = town,
            FilterRiskRating = riskRating,
            AvailableTowns = availableTowns
        };

        _logger.LogInformation("Dashboard metrics calculated: Inspections={InspectionsCount}, Failed={FailedCount}, Overdue={OverdueCount}", 
            inspectionsThisMonth, failedInspectionsThisMonth, overdueFollowUps);

        return View(viewModel);
    }
}
