namespace WelpenWache.Core.Database.Models;

public class InternTeamAssignment {
    public Guid Id { get; set; }

    public Guid InternId { get; set; }
    public Intern Intern { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public DateTime Date { get; set; }
}
