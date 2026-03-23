using Food.Domain.Models;

namespace Food.MVC.ViewModels;

public class DashboardViewModel
{
    public int InspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueFollowUps { get; set; }
    
    public string? FilterTown { get; set; }
    public RiskRating? FilterRiskRating { get; set; }
    
    public List<string> AvailableTowns { get; set; } = new();
}
