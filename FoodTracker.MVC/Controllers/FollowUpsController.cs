using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Food.MVC.Data;
using Food.Domain.Models;

namespace Food.MVC.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var followUps = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i!.Premises)
            .OrderBy(f => f.DueDate)
            .ToListAsync();
        
        _logger.LogInformation("Follow-ups list viewed by user: {UserName}", User.Identity?.Name);
        return View(followUps);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create()
    {
        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .Select(i => new { i.Id, DisplayText = $"{i.Premises!.Name} - {i.InspectionDate:d}" })
            .ToListAsync();
        
        ViewData["InspectionId"] = new SelectList(inspections, "Id", "DisplayText");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create([Bind("InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
    {
        var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
        
        if (inspection != null && followUp.DueDate < inspection.InspectionDate)
        {
            _logger.LogWarning("Follow-up due date before inspection date: InspectionId={InspectionId}, InspectionDate={InspectionDate}, DueDate={DueDate}", 
                followUp.InspectionId, inspection.InspectionDate, followUp.DueDate);
            ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
        }

        if (followUp.Status == FollowUpStatus.Closed && !followUp.ClosedDate.HasValue)
        {
            _logger.LogWarning("Attempt to close follow-up without closed date: InspectionId={InspectionId}", followUp.InspectionId);
            ModelState.AddModelError("ClosedDate", "Closed date is required when status is Closed.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(followUp);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Follow-up created: ID={FollowUpId}, InspectionId={InspectionId}, DueDate={DueDate}, Status={Status}, User={UserName}", 
                    followUp.Id, followUp.InspectionId, followUp.DueDate, followUp.Status, User.Identity?.Name);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating follow-up for InspectionId={InspectionId}", followUp.InspectionId);
                ModelState.AddModelError("", "Unable to save follow-up. Please try again.");
            }
        }

        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .Select(i => new { i.Id, DisplayText = $"{i.Premises!.Name} - {i.InspectionDate:d}" })
            .ToListAsync();
        
        ViewData["InspectionId"] = new SelectList(inspections, "Id", "DisplayText", followUp.InspectionId);
        return View(followUp);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp == null)
        {
            _logger.LogWarning("Follow-up not found for edit: ID={FollowUpId}", id);
            return NotFound();
        }

        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .Select(i => new { i.Id, DisplayText = $"{i.Premises!.Name} - {i.InspectionDate:d}" })
            .ToListAsync();
        
        ViewData["InspectionId"] = new SelectList(inspections, "Id", "DisplayText", followUp.InspectionId);
        return View(followUp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
    {
        if (id != followUp.Id)
        {
            return NotFound();
        }

        if (followUp.Status == FollowUpStatus.Closed && !followUp.ClosedDate.HasValue)
        {
            _logger.LogWarning("Attempt to close follow-up without closed date: ID={FollowUpId}", followUp.Id);
            ModelState.AddModelError("ClosedDate", "Closed date is required when status is Closed.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(followUp);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Follow-up updated: ID={FollowUpId}, InspectionId={InspectionId}, Status={Status}, User={UserName}", 
                    followUp.Id, followUp.InspectionId, followUp.Status, User.Identity?.Name);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!FollowUpExists(followUp.Id))
                {
                    _logger.LogWarning("Follow-up not found during update: ID={FollowUpId}", followUp.Id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating follow-up: ID={FollowUpId}", followUp.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating follow-up: ID={FollowUpId}", followUp.Id);
                ModelState.AddModelError("", "Unable to update follow-up. Please try again.");
            }
            
            return RedirectToAction(nameof(Index));
        }

        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .Select(i => new { i.Id, DisplayText = $"{i.Premises!.Name} - {i.InspectionDate:d}" })
            .ToListAsync();
        
        ViewData["InspectionId"] = new SelectList(inspections, "Id", "DisplayText", followUp.InspectionId);
        return View(followUp);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var followUp = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i!.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (followUp == null)
        {
            _logger.LogWarning("Follow-up not found for delete: ID={FollowUpId}", id);
            return NotFound();
        }

        return View(followUp);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Follow-up deleted: ID={FollowUpId}, User={UserName}", 
                    id, User.Identity?.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting follow-up: ID={FollowUpId}", id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool FollowUpExists(int id)
    {
        return _context.FollowUps.Any(e => e.Id == id);
    }
}
