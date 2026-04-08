using System;

namespace DHM.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#594AE2";
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty; // denormalized for display
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
