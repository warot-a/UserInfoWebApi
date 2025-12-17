namespace UserInfoWebApi.Model.Response;

public class UserAccount
{
    public UserAccount()
    {
    }

    public string Uuid { get; set; }

    public string LocationAccountId { get; set; }

    public string NearestLegalEntityId { get; set; }

    public string UltimateParentId { get; set; }

    public bool IsInternal { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string JobRole { get; set; }

    public string AccountName { get; set; }

}