using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Food.MVC.Data;
using Food.Domain.Models;

namespace Food.MVC.Controllers;

[Authorize]
public class PremisesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PremisesController> _logger;

    public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var premises = await _context.Premises
            .Include(p => p.Inspections)
            .OrderBy(p => p.Name)
            .ToListAsync();
        
        _logger.LogInformation("Premises list viewed by user: {UserName}", User.Identity?.Name);
        return View(premises);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            _logger.LogWarning("Premises details accessed with null ID");
            return NotFound();
        }

        var premises = await _context.Premises
            .Include(p => p.Inspections)
            .ThenInclude(i => i.FollowUps)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (premises == null)
        {
            _logger.LogWarning("Premises not found: ID={PremisesId}", id);
            return NotFound();
        }

        _logger.LogInformation("Premises details viewed: ID={PremisesId}, Name={Name}", 
            premises.Id, premises.Name);
        return View(premises);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Name,Address,Town,RiskRating")] Premises premises)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(premises);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Premises created: ID={PremisesId}, Name={Name}, Town={Town}, RiskRating={RiskRating}, User={UserName}", 
                    premises.Id, premises.Name, premises.Town, premises.RiskRating, User.Identity?.Name);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating premises: Name={Name}", premises.Name);
                ModelState.AddModelError("", "Unable to save premises. Please try again.");
            }
        }
        
        return View(premises);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var premises = await _context.Premises.FindAsync(id);
        if (premises == null)
        {
            _logger.LogWarning("Premises not found for edit: ID={PremisesId}", id);
            return NotFound();
        }
        
        return View(premises);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
    {
        if (id != premises.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(premises);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Premises updated: ID={PremisesId}, Name={Name}, Town={Town}, RiskRating={RiskRating}, User={UserName}", 
                    premises.Id, premises.Name, premises.Town, premises.RiskRating, User.Identity?.Name);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PremisesExists(premises.Id))
                {
                    _logger.LogWarning("Premises not found during update: ID={PremisesId}", premises.Id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error updating premises: ID={PremisesId}", premises.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating premises: ID={PremisesId}", premises.Id);
                ModelState.AddModelError("", "Unable to update premises. Please try again.");
                return View(premises);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        return View(premises);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var premises = await _context.Premises
            .Include(p => p.Inspections)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (premises == null)
        {
            return NotFound();
        }

        return View(premises);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises != null)
            {
                _context.Premises.Remove(premises);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Premises deleted: ID={PremisesId}, Name={Name}, User={UserName}", 
                    id, premises.Name, User.Identity?.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting premises: ID={PremisesId}", id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PremisesExists(int id)
    {
        return _context.Premises.Any(e => e.Id == id);
    }
}
