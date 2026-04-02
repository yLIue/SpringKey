using SpringKey.View;
using System.Windows;
using SpringKey.ViewModel;

namespace SpringKey.Services;

public class DialogService(IPromptService promptService) : IDialogService
{
    private readonly IPromptService _promptService = promptService;
    
    public void ShowAddFileView()
    {
        var vm = new AddFileViewModel(this, _promptService);
        var win = new AddFileView { DataContext = vm };
        win.ShowDialog();
    }

    public void CloseAddFileView()
    {
        var win = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
        if (win != null)
        {
            win.Close();
        }
    }
}