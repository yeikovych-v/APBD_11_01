using Microsoft.AspNetCore.Mvc;

namespace APBD_11_01.Controller;

[ApiController]
public class ErrorsController
{
    
    [HttpGet]
    [Route("/api/v1/exception")]
    public IResult GetException()
    {
        throw new Exception("HA-HA-HA!!!");
    }
}