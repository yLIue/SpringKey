using Microsoft.Win32;
using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Struct;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SpringKey.ViewModel;

public class ImportExportViewModel : ViewModelBase
{
    private readonly string _basePath;

    public ObservableCollection<UserInfo> Users { get; } = new();

    private UserInfo? _selectedUser;
    public UserInfo? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (_selectedUser == value) return;
            _selectedUser = value;
            OnPropertyChanged(nameof(SelectedUser));
            (ExportBackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ExportBackupCommand { get; }
    public ICommand ImportBackupCommand { get; }

    public ImportExportViewModel(string basePath)
    {
        _basePath = basePath;
        ExportBackupCommand = new RelayCommand(ExportBackup, CanExport);
        ImportBackupCommand = new RelayCommand(ImportBackup);
        LoadUsers();
    }

    private void LoadUsers()
    {
        try
        {
            var list = BackupFile.GetUserList(_basePath);
            Users.Clear();
            foreach (var u in list) Users.Add(u);
            StatusMessage = $"检测到 {Users.Count} 个用户";
            if (Users.Count > 0) SelectedUser = Users[0];
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载用户失败：{ex.Message}";
        }
    }

    private bool CanExport() => SelectedUser != null;

    private void ExportBackup()
    {
        if (SelectedUser == null)
        {
            MessageBox.Show("请先选择要导出的用户。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new SaveFileDialog
        {
            Title = "导出为 .skbackup 文件",
            Filter = "SK Backup (*.skbackup)|*.skbackup",
            DefaultExt = ".skbackup",
            FileName = $"{SelectedUser.UserName}.skbackup",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            BackupFile.ExportUser(SelectedUser.Hash, _basePath, dlg.FileName);
            MessageBox.Show($"导出成功：{dlg.FileName}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"导出：{SelectedUser.UserName} → {dlg.FileName}";
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
            BackupFile.ImportUser(_basePath, dlg.FileName);
            MessageBox.Show($"导入完成：{dlg.FileName}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusMessage = $"已导入：{dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"导入失败：{ex.Message}";
        }
    }
}
