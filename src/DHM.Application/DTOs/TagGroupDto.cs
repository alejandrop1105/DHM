using System;

namespace DHM.Application.DTOs;

public class TagGroupDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#594AE2";
    public int SortOrder { get; set; } = 0;
    public int TagCount { get; set; } = 0;
}
