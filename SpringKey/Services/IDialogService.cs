using SpringKey.Files;

namespace SpringKey.Services;
using Struct;

public interface IDialogService
{
    void ShowAddFileView(AddKeyParameter addKeyParameter);
    void CloseAddFileView();
    void ShowImportExportView(string basePath, string userHash, string userName);
    void ShowBackupSettingsView(AutoBackupService backupService);
}