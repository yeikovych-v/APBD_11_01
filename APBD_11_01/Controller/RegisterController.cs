using APBD_11_01.Dto;
using APBD_11_01.Service;
using Microsoft.AspNetCore.Mvc;

namespace APBD_11_01.Controller;

[ApiController]
public class RegisterController(UserService service)
{
    
    [HttpPost]
    [Route("/api/v1/register")]
    public IResult RegisterUser(UserDto dto)
    {
        var user = service.GetByUsername(dto.Username);
        if (user != null)
        {
            return Results.Conflict("User with given username already exists!!");
        }

        var created = service.CreateAndGet(dto);
        return Results.Ok(created);
    }
}