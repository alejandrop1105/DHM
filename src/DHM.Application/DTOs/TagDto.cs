using System;

namespace DHM.Application.DTOs;

public class TagDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#594AE2";
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
}
