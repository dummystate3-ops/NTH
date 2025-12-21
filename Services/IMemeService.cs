using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public interface IMemeService
{
    Task<MemeResponse> GenerateAsync(MemeRequest request, CancellationToken cancellationToken = default);
}
