using SpringKey.Files;
using SpringKey.View;
using System.Windows;
using SpringKey.Struct;
using SpringKey.ViewModel;

namespace SpringKey.Services;

public class DialogService(IPromptService promptService) : IDialogService
{
    private readonly IPromptService _promptService = promptService;

    public void ShowAddFileView(AddKeyParameter addKeyParameter)
    {
        var vm = new AddFileViewModel(this, _promptService, addKeyParameter);
        var win = new AddFileView { DataContext = vm };
        win.Owner = Application.Current.MainWindow;
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

    public void ShowImportExportView(string basePath, string userHash, string userName)
    {
        var win = new ImportExportWindow(basePath, userHash, userName);
        win.Owner = Application.Current.MainWindow;
        win.ShowDialog();
    }

    public void ShowBackupSettingsView(AutoBackupService backupService)
    {
        var win = new BackupSettingsWindow(backupService);
        win.Owner = Application.Current.MainWindow;
        win.ShowDialog();
    }
}