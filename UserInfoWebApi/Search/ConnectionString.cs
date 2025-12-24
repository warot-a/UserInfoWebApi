namespace UserInfoWebApi.Search;

public class ConnectionString
{
    public string Hostname { get; set; }
    public bool Secure { get; set; }

    public override string ToString()
    {
        return $"{(Secure ? "https" : "http")}://{Hostname}/";
    }
}