using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public interface IAiService
{
    Task<AiTextResponse> DraftAsync(AiDraftRequest request);
    Task<AiTextResponse> SummarizeAsync(AiSummarizeRequest request);
    Task<AiTextResponse> ImproveAsync(AiImproveRequest request);
    Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request);
}
