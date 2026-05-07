using SpringKey.ViewModel;
using System.Windows;

namespace SpringKey.View;

public partial class ImportExportWindow : Window
{
    public ImportExportWindow(string basePath, string userHash, string userName)
    {
        InitializeComponent();
        DataContext = new ImportExportViewModel(basePath, userHash, userName);
    }
}
