namespace DHM.Application.DTOs;

public class SavedQueryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SqlText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Guid> SelectedTenantIds { get; set; } = new();
    public string? GroupName { get; set; }
}
