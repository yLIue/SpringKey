using SpringKey.MVVM;

namespace SpringKey.Struct;

public class BindingItem : ViewModelBase
{
    private string _type = "";
    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private string _value = "";
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
}
