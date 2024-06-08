﻿namespace APBD_11_01.Dto;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

    public TokenResponse()
    {
    }

    public TokenResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}