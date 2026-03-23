using System.ComponentModel.DataAnnotations;

namespace Food.Domain.Models;

public class Premises
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Town { get; set; } = string.Empty;
    
    public RiskRating RiskRating { get; set; }
    
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}
