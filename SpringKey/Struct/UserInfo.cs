namespace SpringKey.Struct;

public class UserInfo
{
    public string UserName { get; set; }
    public string Hash { get; set; }

    public UserInfo(string hash, string userName)
    {
        Hash = hash;
        UserName = userName;
    }
}
