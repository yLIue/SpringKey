namespace SpringKey.Struct;

public class BackupSettings
{
    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 30;
    public DateTime? LastBackup { get; set; }
}
