using SpringKey.MVVM;
using SpringKey.Services;

namespace SpringKey.ViewModel;

class AddFileViewModel : ViewModelBase
{

    public AddFileViewModel() : this(new DialogService())
    {
        
    }
    
    public AddFileViewModel(IDialogService dialogService)
    {
        
    }
}