using System.ComponentModel;
using SpringKey.Struct;

namespace SpringKey.Test.Struct;

public class BindingItemTests
{
    private static BindingItem CreateItem()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new BindingItem();
    }

    [Fact]
    public void DefaultConstructor_HasEmptyFields()
    {
        var item = CreateItem();
        Assert.Equal("", item.Type);
        Assert.Equal("", item.Value);
    }

    [Fact]
    public void Type_SetProperty_RaisesChanged()
    {
        var item = CreateItem();
        string? changed = null;
        item.PropertyChanged += (_, args) => changed = args.PropertyName;
        item.Type = "email";
        Assert.Equal("email", item.Type);
        Assert.Equal(nameof(BindingItem.Type), changed);
    }

    [Fact]
    public void Value_SetProperty_RaisesChanged()
    {
        var item = CreateItem();
        string? changed = null;
        item.PropertyChanged += (_, args) => changed = args.PropertyName;
        item.Value = "test@test.com";
        Assert.Equal("test@test.com", item.Value);
        Assert.Equal(nameof(BindingItem.Value), changed);
    }

    [Fact]
    public void Type_SameValue_DoesNotRaiseChanged()
    {
        var item = CreateItem();
        item.Type = "phone";
        var raised = false;
        item.PropertyChanged += (_, _) => raised = true;
        item.Type = "phone";
        Assert.False(raised);
    }
}
