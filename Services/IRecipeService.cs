using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public interface IRecipeService
{
    Task<RecipeResponse> GenerateAsync(RecipeRequest request, CancellationToken cancellationToken = default);
}
