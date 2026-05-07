using Microsoft.Win32;
using SpringKey.Files;
using SpringKey.MVVM;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SpringKey.ViewModel;

public class ImportExportViewModel : ViewModelBase
{
    private readonly string _basePath;
    private readonly string _userHash;
    private readonly string _userName;

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string UserName => _userName;

    public ICommand ExportBackupCommand { get; }
    public ICommand ImportBackupCommand { get; }

    public ImportExportViewModel(string basePath, string userHash, string userName)
    {
        _basePath = basePath;
        _userHash = userHash;
        _userName = userName;
        ExportBackupCommand = new RelayCommand(ExportBackup);
        ImportBackupCommand = new RelayCommand(ImportBackup);
    }

    private void ExportBackup()
    {
        var dlg = new SaveFileDialog
        {
            Title = "导出为 .skbackup 文件",
            Filter = "SK Backup (*.skbackup)|*.skbackup",
            DefaultExt = ".skbackup",
            FileName = $"{_userName}.skbackup",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            BackupFile.ExportUser(_userHash, _basePath, dlg.FileName);
            StatusMessage = $"已导出到 {dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"导出失败：{ex.Message}";
        }
    }

    private void ImportBackup()
    {
        var dlg = new OpenFileDialog
        {
            Title = "选择 .skbackup 文件以导入",
            Filter = "SK Backup (*.skbackup)|*.skbackup",
            DefaultExt = ".skbackup",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            string linkPath = Path.Combine(_basePath, "link", $"{_userHash}.sklink");
            string linkValue = File.ReadAllText(linkPath, Encoding.UTF8);
            string userPath = linkValue.Split(' ')[1];
            BackupFile.ImportUser(_basePath, dlg.FileName, userPath);
            StatusMessage = $"已从 {dlg.FileName} 导入";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"导入失败：{ex.Message}";
        }
    }
}
