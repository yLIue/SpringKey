namespace SpringKey.Services;

public class PromptService : IPromptService
{
    public event Action<string>? PromptRequested;
    
    public void Show(string message)
    {
        PromptRequested?.Invoke(message);
    }
}