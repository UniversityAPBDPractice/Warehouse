using System.ComponentModel.DataAnnotations;

namespace Warehouse.Entities;

public class ProductWarehouse
{
    [Required]
    [Key]
    public int IdProductWarehouse { get; set; }
    
    [Required]
    [Key]
    public int IdWarehouse { get; set; }
    
    [Required]
    [Key]
    public int IdProduct { get; set; }
    
    [Required]
    [Key]
    public int IdOrder { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
    [Required]
    public int Price { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
}