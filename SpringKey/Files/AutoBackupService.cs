using SpringKey.Struct;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace SpringKey.Files;

public class AutoBackupService : IDisposable
{
    private readonly string _basePath;
    private readonly string _backupDir;
    private readonly string _settingsPath;
    private System.Timers.Timer? _timer;
    private BackupSettings _settings = new();
    private volatile bool _isBackingUp;

    public event Action<string>? StatusChanged;
    public event Action? BackupsChanged;

    public bool IsEnabled
    {
        get => _settings.Enabled;
        set
        {
            _settings.Enabled = value;
            SaveSettings();
            if (value) StartTimer(); else StopTimer();
        }
    }

    public int IntervalMinutes
    {
        get => _settings.IntervalMinutes;
        set
        {
            _settings.IntervalMinutes = value;
            SaveSettings();
            if (_settings.Enabled) { StopTimer(); StartTimer(); }
        }
    }

    public DateTime? LastBackup => _settings.LastBackup;

    public AutoBackupService(string basePath)
    {
        _basePath = basePath;
        _backupDir = Path.Combine(basePath, "backup");
        _settingsPath = Path.Combine(_backupDir, ".settings.json");
        Directory.CreateDirectory(_backupDir);
        LoadSettings();
        if (_settings.Enabled)
            StartTimer();
    }

    private void LoadSettings()
    {
        if (!File.Exists(_settingsPath)) return;
        try
        {
            _settings = JsonSerializer.Deserialize<BackupSettings>(File.ReadAllText(_settingsPath)) ?? new BackupSettings();
        }
        catch
        {
            _settings = new BackupSettings();
        }
    }

    private void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(_settings), Encoding.UTF8);
    }

    private void StartTimer()
    {
        _timer?.Dispose();
        _timer = new System.Timers.Timer(_settings.IntervalMinutes * 60 * 1000);
        _timer.AutoReset = true;
        _timer.Elapsed += (_, _) => BackupAll();
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public void BackupAll()
    {
        if (_isBackingUp) return;
        _isBackingUp = true;
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(_backupDir, $"auto_{timestamp}");
            Directory.CreateDirectory(backupPath);

            string linksDir = Path.Combine(backupPath, "links");
            Directory.CreateDirectory(linksDir);

            var users = BackupFile.GetUserList(_basePath);
            int count = 0;
            foreach (var user in users)
            {
                try
                {
                    string srcLink = Path.Combine(_basePath, "link", $"{user.Hash}.sklink");
                    string dstLink = Path.Combine(linksDir, $"{user.Hash}.sklink");
                    File.Copy(srcLink, dstLink, true);

                    string backupFile = Path.Combine(backupPath, $"{user.UserName}.skbackup");
                    BackupFile.ExportUser(user.Hash, _basePath, backupFile);
                    count++;
                }
                catch { }
            }

            _settings.LastBackup = DateTime.Now;
            SaveSettings();
            StatusChanged?.Invoke($"备份完成：{count} 个用户  {timestamp}");
            BackupsChanged?.Invoke();
        }
        finally
        {
            _isBackingUp = false;
        }
    }

    public List<BackupInfo> GetBackups()
    {
        var backups = new List<BackupInfo>();
        if (!Directory.Exists(_backupDir)) return backups;

        foreach (string dir in Directory.GetDirectories(_backupDir))
        {
            string dirName = Path.GetFileName(dir);
            if (!dirName.StartsWith("auto_")) continue;

            string dateStr = dirName[5..];
            DateTime createdAt;
            if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd_HH-mm-ss", null,
                    System.Globalization.DateTimeStyles.None, out createdAt))
                createdAt = Directory.GetCreationTime(dir);

            int userCount = Directory.GetFiles(dir, "*.skbackup").Length;
            backups.Add(new BackupInfo
            {
                DirectoryPath = dir,
                CreatedAt = createdAt,
                UserCount = userCount
            });
        }

        backups.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
        return backups;
    }

    public void RestoreBackup(string backupPath)
    {
        string linksDir = Path.Combine(backupPath, "links");
        if (Directory.Exists(linksDir))
        {
            string targetLinkDir = Path.Combine(_basePath, "link");
            Directory.CreateDirectory(targetLinkDir);
            foreach (string linkFile in Directory.GetFiles(linksDir, "*.sklink"))
            {
                string fileName = Path.GetFileName(linkFile);
                File.Copy(linkFile, Path.Combine(targetLinkDir, fileName), true);
            }
        }

        foreach (string backupFile in Directory.GetFiles(backupPath, "*.skbackup"))
        {
            string userName = Path.GetFileNameWithoutExtension(backupFile);
            string? userHash = FindUserHash(userName);
            if (userHash == null) continue;

            string linkPath = Path.Combine(_basePath, "link", $"{userHash}.sklink");
            if (!File.Exists(linkPath)) continue;

            string linkValue = File.ReadAllText(linkPath, Encoding.UTF8);
            string userPath = linkValue.Split(' ')[1];
            BackupFile.ImportUser(_basePath, backupFile, userPath);
        }

        StatusChanged?.Invoke("恢复完成");
        BackupsChanged?.Invoke();
    }

    private string? FindUserHash(string userName)
    {
        foreach (var user in BackupFile.GetUserList(_basePath))
            if (user.UserName == userName)
                return user.Hash;
        return null;
    }

    public void Dispose()
    {
        StopTimer();
    }
}
