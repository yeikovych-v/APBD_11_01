using APBD_11_01.Dto;
using APBD_11_01.Service;
using Microsoft.AspNetCore.Mvc;

namespace APBD_11_01.Controller;

[ApiController]
public class LoginController(UserService service)
{

    [HttpPost]
    [Route("/api/v1/login")]
    public IResult GetLoginToken(UserDto dto)
    {
        var userOpt = service.GetByUsername(dto.Username);
        if (userOpt == null)
        {
            return Results.BadRequest("Client with given credentials does not exist!");
        }

        var isCorrectCredentials = service.Authenticate(dto);
        if (!isCorrectCredentials)
        {
            return Results.BadRequest("Invalid password!");
        }

        var credentialsDto = service.GetCredentialsDtoFor(dto);
        return Results.Ok(credentialsDto);
    }
    
    [HttpPost]
    [Route("/api/v1/refresh")]
    public IResult GetAccessFromRefreshToken(TokenRequest token)
    {
        var value = service.GetAccessToken(token.RefreshToken);
        return value == null ? Results.BadRequest("Invalid Refresh Token!") : Results.Ok(value);
    }
}