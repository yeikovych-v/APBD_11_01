namespace APBD_11_01.Models;

public class AppUser
{
    public long IdUser { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RefreshToken { get; set; }

    public AppUser()
    {
    }

    public AppUser(string username, string password, string refreshToken)
    {
        Username = username;
        Password = password;
        RefreshToken = refreshToken;
    }
}