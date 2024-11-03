using System.ComponentModel.DataAnnotations;
using Ploch.Data.Model;

namespace Ploch.Data.EFCore.Tests;

public class TestEntity : IHasId<int>, INamed
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;
}