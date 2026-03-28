using SpringKey.View;

namespace SpringKey.Services;

public class DialogService : IDialogService
{
    public void ShowAddFileView()
    {
        var win = new AddFileView();
        win.ShowDialog();
    }
}