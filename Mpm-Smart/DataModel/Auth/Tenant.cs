using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel.Auth;

public class Tenant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Guid { get; set; }
    
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string BannerUrl { get; set; } = null!;

    [StringLength(200)]
    public string IconUrl { get; set; } = null!;
}