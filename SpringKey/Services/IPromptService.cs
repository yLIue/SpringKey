namespace SpringKey.Services;

public interface IPromptService
{
    event Action<string>? PromptRequested;
    void Show(string message);
}