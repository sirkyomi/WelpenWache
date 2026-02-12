using WelpenWache.Core.Features.Intern.Models;

namespace WelpenWache.Core.Features.Intern;

public interface IInternService {
    Task<Guid> CreateInternAsync(InternCreateRequest request);
    Task<List<InternDto>> GetInternsAsync();
    Task<InternDto> GetInternAsync(Guid id);
    
    Task UpdateInternAsync(InternDto dto);
    Task DeleteInternAsync(Guid id);
}