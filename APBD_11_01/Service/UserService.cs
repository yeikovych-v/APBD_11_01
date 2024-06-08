using APBD_11_01.Context;
using APBD_11_01.Dto;
using APBD_11_01.Models;

namespace APBD_11_01.Service;

public class UserService(ApiContext context, TokenService tokenService)
{
    public AppUser? GetByUsername(string username)
    {
        return context.Users.FirstOrDefault(u => u.Username == username);
    }

    public AppUser CreateAndGet(UserDto dto)
    {
        var refreshToken = CreateToken();
        var user = new AppUser(dto.Username, dto.Password, refreshToken);
        
        context.Users.Add(user);
        context.SaveChanges();
        
        return user;
    }

    public TokenResponse GetCredentialsDtoFor(UserDto dto)
    {
        return GetCredentialsFor(dto.Username);
    }

    private TokenResponse GetCredentialsFor(string username)
    {
        var user = GetByUsername(username);
        if (user == null)
        {
            throw new InvalidOperationException("User was deleted from database during token creation process.");
        }

        var credentials = new TokenResponse(tokenService.GenerateAccessToken(user.RefreshToken), user.RefreshToken);

        return credentials;
    }

    private string CreateToken()
    {
        var value = tokenService.GenerateRefreshToken();
        return value;
    }

    public bool Authenticate(UserDto dto)
    {
        var user = GetByUsername(dto.Username);
        if (user == null) 
            throw new InvalidOperationException("User was deleted from database during authentication process.");
        return user.Password == dto.Password;
    }

    public string? GetAccessToken(string refreshToken)
    {
        return tokenService.GenerateAccessToken(refreshToken);
    }
}