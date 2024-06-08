using System.Security.Cryptography;
using APBD_11_01.Context;

namespace APBD_11_01.Service;

public class TokenService(ApiContext context)
{

    public string GenerateRefreshToken()
    {
        return GenerateToken();
    }

    public string GenerateAccessToken(string token)
    {
        var refreshToken = context.Users.FirstOrDefault(u => u.RefreshToken == token);
        if (refreshToken == null)
            throw new InvalidOperationException("Cannot generate access token if Refresh Token is null.");

        return GenerateToken();
    }

    private string GenerateToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}