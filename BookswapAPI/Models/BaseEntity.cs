using System.ComponentModel.DataAnnotations.Schema;

namespace BookswapAPI.Models;

public class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public DateTime CreateAt { get; set; }
    public DateTime CreateBy { get; set; }
    
    public DateTime? UpdateAt { get; set; }
    public DateTime? UpdateBy { get; set; } 
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    public DateTime? DeletedBy { get; set; }
}