namespace SpringKey.Struct;

public class BackupInfo
{
    public string DirectoryPath { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public string DisplayName => $"{CreatedAt:yyyy-MM-dd HH:mm:ss}  |  {UserCount} 个用户";
}
