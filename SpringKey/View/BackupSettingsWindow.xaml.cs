using SpringKey.Files;
using SpringKey.ViewModel;
using System.Windows;

namespace SpringKey.View;

public partial class BackupSettingsWindow : Window
{
    public BackupSettingsWindow(AutoBackupService backupService)
    {
        InitializeComponent();
        DataContext = new BackupSettingsViewModel(backupService);
    }
}
