using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public interface IRateComparisonService
{
    Task<RateComparisonResponse> CompareAsync(RateComparisonRequest request, CancellationToken cancellationToken = default);
}
