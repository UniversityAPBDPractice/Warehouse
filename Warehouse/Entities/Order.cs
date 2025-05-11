using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Warehouse.Entities;

public class Order
{
    [Required]
    [Key]
    public int IdOrder { get; set; }
    
    [Required]
    [Key]
    public int IdProduct { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime FulfilledAt { get; set; }
}