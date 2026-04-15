namespace DHM.Application.DTOs;

public class QueryGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QueryCount { get; set; } // calculado en el servicio
}
