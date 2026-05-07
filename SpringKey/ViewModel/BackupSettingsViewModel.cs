using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Struct;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SpringKey.ViewModel;

public class BackupSettingsViewModel : ViewModelBase
{
    private readonly AutoBackupService _backupService;

    public ICommand BackupNowCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand DeleteBackupCommand { get; }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            SetProperty(ref _isEnabled, value);
            _backupService.IsEnabled = value;
        }
    }

    private int _selectedIntervalIndex;
    public int SelectedIntervalIndex
    {
        get => _selectedIntervalIndex;
        set
        {
            if (value < 0 || value >= Intervals.Count) return;
            SetProperty(ref _selectedIntervalIndex, value);
            _backupService.IntervalMinutes = Intervals[value].Minutes;
        }
    }

    public record IntervalOption(string Label, int Minutes);
    public List<IntervalOption> Intervals { get; } = new()
    {
        new("15 分钟", 15),
        new("30 分钟", 30),
        new("1 小时", 60),
        new("2 小时", 120),
        new("6 小时", 360),
        new("12 小时", 720),
        new("24 小时", 1440),
    };

    private ObservableCollection<BackupInfo> _backups = new();
    public ObservableCollection<BackupInfo> Backups
    {
        get => _backups;
        set => SetProperty(ref _backups, value);
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private BackupInfo? _selectedBackup;
    public BackupInfo? SelectedBackup
    {
        get => _selectedBackup;
        set => SetProperty(ref _selectedBackup, value);
    }

    public BackupSettingsViewModel(AutoBackupService backupService)
    {
        _backupService = backupService;
        _isEnabled = backupService.IsEnabled;

        int minutes = backupService.IntervalMinutes;
        int idx = Intervals.FindIndex(i => i.Minutes == minutes);
        _selectedIntervalIndex = idx >= 0 ? idx : 1;

        BackupNowCommand = new RelayCommand(BackupNow);
        RestoreCommand = new RelayCommand<BackupInfo>(Restore);
        DeleteBackupCommand = new RelayCommand<BackupInfo>(DeleteBackup);

        _backupService.StatusChanged += msg =>
            Application.Current.Dispatcher.Invoke(() => StatusMessage = msg);
        _backupService.BackupsChanged += () =>
            Application.Current.Dispatcher.Invoke(RefreshBackups);

        RefreshBackups();
    }

    private void BackupNow()
    {
        StatusMessage = "正在备份...";
        Task.Run(() => _backupService.BackupAll());
    }

    private void Restore(BackupInfo? backup)
    {
        if (backup == null) return;
        if (!View.ConfirmDialog.Show(
            $"确定要恢复 \"{backup.DisplayName}\" 的备份吗？\n当前数据将被删除并替换为备份数据。",
            "恢复备份"))
            return;

        try
        {
            _backupService.RestoreBackup(backup.DirectoryPath);
            StatusMessage = $"已恢复: {backup.DisplayName}";
            RefreshBackups();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"恢复失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"恢复失败：{ex.Message}";
        }
    }

    private void DeleteBackup(BackupInfo? backup)
    {
        if (backup == null) return;
        if (!View.ConfirmDialog.Show(
            $"确定要删除备份 \"{backup.DisplayName}\" 吗？",
            "删除备份"))
            return;

        try
        {
            Directory.Delete(backup.DirectoryPath, true);
            StatusMessage = $"已删除: {backup.DisplayName}";
            RefreshBackups();
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败：{ex.Message}";
        }
    }

    private void RefreshBackups()
    {
        Backups = new ObservableCollection<BackupInfo>(_backupService.GetBackups());
    }
}
