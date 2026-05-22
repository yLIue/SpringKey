using System.Windows.Input;
using SpringKey.MVVM;

namespace SpringKey.Test.MVVM;

public class RelayCommandTests
{
    [Fact]
    public void Execute_CallsAction()
    {
        var called = false;
        var cmd = new RelayCommand(() => called = true);
        cmd.Execute(null);
        Assert.True(called);
    }

    [Fact]
    public void CanExecute_NullPredicate_ReturnsTrue()
    {
        var cmd = new RelayCommand(() => { });
        Assert.True(cmd.CanExecute(null));
        Assert.True(cmd.CanExecute("any parameter"));
    }

    [Fact]
    public void CanExecute_WithPredicate_ReturnsPredicateResult()
    {
        var cmd = new RelayCommand(() => { }, () => false);
        Assert.False(cmd.CanExecute(null));
    }

    [Fact]
    public void CanExecute_PredicateChanges_ReflectsNewValue()
    {
        var flag = true;
        var cmd = new RelayCommand(() => { }, () => flag);
        Assert.True(cmd.CanExecute(null));
        flag = false;
        Assert.False(cmd.CanExecute(null));
    }

    [Fact]
    public void Constructor_NullExecute_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null!));
    }

    [Fact]
    public void CanExecuteChanged_CanSubscribe()
    {
        var cmd = new RelayCommand(() => { });
        var fired = false;
        EventHandler handler = (_, _) => fired = true;
        cmd.CanExecuteChanged += handler;
        cmd.CanExecuteChanged -= handler;
        Assert.False(fired); // subscription/unsubscription didn't throw
    }
}

public class RelayCommandTTests
{
    [Fact]
    public void Execute_PassesParameter()
    {
        string? received = null;
        var cmd = new RelayCommand<string?>(p => received = p);
        cmd.Execute("hello");
        Assert.Equal("hello", received);
    }

    [Fact]
    public void Execute_NullParameter()
    {
        string? received = "not null";
        var cmd = new RelayCommand<string?>(p => received = p);
        cmd.Execute(null);
        Assert.Null(received);
    }

    [Fact]
    public void CanExecute_NullPredicate_ReturnsTrue()
    {
        var cmd = new RelayCommand<string?>(_ => { });
        Assert.True(cmd.CanExecute(null));
        Assert.True(cmd.CanExecute("anything"));
    }

    [Fact]
    public void CanExecute_PassesParameterToPredicate()
    {
        string? received = null;
        var cmd = new RelayCommand<string?>(_ => { }, p => { received = p; return true; });
        cmd.CanExecute("test");
        Assert.Equal("test", received);
    }

    [Fact]
    public void CanExecute_WithPredicate_ReturnsPredicateResult()
    {
        var cmd = new RelayCommand<string?>(_ => { }, p => p is string s && s.Length > 3);
        Assert.False(cmd.CanExecute("ab"));
        Assert.True(cmd.CanExecute("abcd"));
    }

    [Fact]
    public void Constructor_NullExecute_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand<string?>(null!));
    }
}
