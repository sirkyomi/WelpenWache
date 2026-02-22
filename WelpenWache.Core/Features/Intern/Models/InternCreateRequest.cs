namespace WelpenWache.Core.Features.Intern.Models;

public record InternCreateRequest(
    string Name,
    string Surname,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyCollection<InternDayAssignmentCreateRequest>? DayAssignments = null
);
