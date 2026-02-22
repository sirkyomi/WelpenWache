namespace WelpenWache.Core.Features.Intern.Models;

public class InternDayAssignmentDto {
    public DateTime Date { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
}
