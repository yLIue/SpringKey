namespace SpringKey.Struct;
using Files;

public class AddKeyParameter
{
    public IndexFile Index;
    public string Group;
    public KeyInfo? ExistingKey;

    public AddKeyParameter(IndexFile index, string group, KeyInfo? existingKey = null)
    {
        Index = index;
        Group = group;
        ExistingKey = existingKey;
    }

    public AddKeyParameter()
    {
        
    }
}