namespace SpringKey.Struct;
using Files;

public class AddKeyParameter
{
    public IndexFile Index;
    public string Group;

    public AddKeyParameter(IndexFile index, string group)
    {
        Index = index;
        Group = group;
    }

    public AddKeyParameter()
    {
        
    }
}