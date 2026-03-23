using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Food.MVC.Data;
using Food.Domain.Models;

namespace Food.MVC.Controllers;

[Authorize]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .OrderByDescending(i => i.InspectionDate)
            .ToListAsync();
        
        _logger.LogInformation("Inspections list viewed by user: {UserName}", User.Identity?.Name);
        return View(inspections);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            _logger.LogWarning("Inspection details accessed with null ID");
            return NotFound();
        }

        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .Include(i => i.FollowUps)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (inspection == null)
        {
            _logger.LogWarning("Inspection not found: ID={InspectionId}", id);
            return NotFound();
        }

        _logger.LogInformation("Inspection details viewed: ID={InspectionId}, PremisesId={PremisesId}", 
            inspection.Id, inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public IActionResult Create()
    {
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create([Bind("PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(inspection);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Inspection created: ID={InspectionId}, PremisesId={PremisesId}, Score={Score}, Outcome={Outcome}, User={UserName}", 
                    inspection.Id, inspection.PremisesId, inspection.Score, inspection.Outcome, User.Identity?.Name);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inspection for PremisesId={PremisesId}", inspection.PremisesId);
                ModelState.AddModelError("", "Unable to save inspection. Please try again.");
            }
        }
        
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null)
        {
            _logger.LogWarning("Inspection not found for edit: ID={InspectionId}", id);
            return NotFound();
        }
        
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
    {
        if (id != inspection.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Inspection updated: ID={InspectionId}, PremisesId={PremisesId}, Score={Score}, Outcome={Outcome}, User={UserName}", 
                    inspection.Id, inspection.PremisesId, inspection.Score, inspection.Outcome, User.Identity?.Name);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!InspectionExists(inspection.Id))
                {
                    _logger.LogWarning("Inspection not found during update: ID={InspectionId}", inspection.Id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating inspection: ID={InspectionId}", inspection.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inspection: ID={InspectionId}", inspection.Id);
                ModelState.AddModelError("", "Unable to update inspection. Please try again.");
                ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
                return View(inspection);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (inspection == null)
        {
            return NotFound();
        }

        return View(inspection);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Inspection deleted: ID={InspectionId}, User={UserName}", 
                    id, User.Identity?.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inspection: ID={InspectionId}", id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool InspectionExists(int id)
    {
        return _context.Inspections.Any(e => e.Id == id);
    }
}
