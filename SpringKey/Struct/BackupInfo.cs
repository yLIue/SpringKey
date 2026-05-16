namespace SpringKey.Struct;

public class BackupInfo
{
    public string DirectoryPath { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public bool IsTemp { get; set; }
    public string DisplayName => IsTemp
        ? $"临时备份  |  {UserCount} 个用户"
        : $"{CreatedAt:yyyy-MM-dd HH:mm:ss}  |  {UserCount} 个用户";
}
