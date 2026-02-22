namespace WelpenWache.Core.Database.Models;

public class Team {
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<InternTeamAssignment> InternAssignments { get; set; } = [];
}
