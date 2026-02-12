namespace WelpenWache.Core.Features.Intern.Models;

public class InternDto {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
};