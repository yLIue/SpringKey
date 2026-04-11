namespace SpringKey.Services;
using Struct;

public interface IDialogService
{
    void ShowAddFileView(AddKeyParameter addKeyParameter);
    void CloseAddFileView();
}