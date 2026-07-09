namespace DoAnCS.Services;

public interface IAIService {
    Task<string> GenerateContent(string prompt, bool isPro = false, string? systemInstruction = null, double? temperature = null, bool responseJson = false);
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
}