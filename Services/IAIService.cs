namespace DoAnCS.Services;

public interface IAIService {
    Task<string> GenerateContent(string prompt, bool isPro = false);
}