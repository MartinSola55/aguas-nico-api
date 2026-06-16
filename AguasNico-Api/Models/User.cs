using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class User : AuditableEntity
{
    [Key]
    public string Id { get; set; }
    public string RoleId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int? TruckNumber { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }
}
