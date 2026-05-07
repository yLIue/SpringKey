using SpringKey.ViewModel;
using System.Windows;

namespace SpringKey.View;

public partial class ImportExportWindow : Window
{
    public ImportExportWindow(string basePath)
    {
        InitializeComponent();
        DataContext = new ImportExportViewModel(basePath);
    }
}
