using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class Role
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
}
