using System.ComponentModel;
using SpringKey.MVVM;

namespace SpringKey.Test.MVVM;

public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _count;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public void RaisePropertyChanged(string name) => OnPropertyChanged(name);
    }

    private static TestViewModel CreateVm()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new TestViewModel();
    }

    [Fact]
    public void SetProperty_UpdatesValue()
    {
        var vm = CreateVm();
        vm.Name = "new value";
        Assert.Equal("new value", vm.Name);
    }

    [Fact]
    public void SetProperty_RaisesPropertyChanged()
    {
        var vm = CreateVm();
        string? changedProperty = null;
        vm.PropertyChanged += (_, args) => changedProperty = args.PropertyName;
        vm.Name = "updated";
        Assert.Equal(nameof(TestViewModel.Name), changedProperty);
    }

    [Fact]
    public void SetProperty_SameValue_DoesNotRaise()
    {
        var vm = CreateVm();
        vm.Name = "same";
        var raised = false;
        vm.PropertyChanged += (_, _) => raised = true;
        vm.Name = "same";
        Assert.False(raised);
    }

    [Fact]
    public void SetProperty_DifferentProperty_RaisesSeparately()
    {
        var vm = CreateVm();
        var changed = new List<string>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName!);
        vm.Name = "a";
        vm.Count = 42;
        Assert.Equal(2, changed.Count);
        Assert.Contains(nameof(TestViewModel.Name), changed);
        Assert.Contains(nameof(TestViewModel.Count), changed);
    }

    [Fact]
    public void SetProperty_ValueType_SameValue_DoesNotRaise()
    {
        var vm = CreateVm();
        vm.Count = 10;
        var raised = false;
        vm.PropertyChanged += (_, _) => raised = true;
        vm.Count = 10;
        Assert.False(raised);
    }

    [Fact]
    public void OnPropertyChanged_RaisesForGivenPropertyName()
    {
        var vm = CreateVm();
        string? changedProperty = null;
        vm.PropertyChanged += (_, args) => changedProperty = args.PropertyName;
        vm.RaisePropertyChanged("CustomProp");
        Assert.Equal("CustomProp", changedProperty);
    }

    [Fact]
    public void PropertyChanged_NoSubscribers_DoesNotThrow()
    {
        var vm = CreateVm();
        vm.Name = "no listeners";
        Assert.Equal("no listeners", vm.Name);
    }
}
