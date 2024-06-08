using System.Security.Cryptography;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#nullable disable

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddXmlSerializerFormatters();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<ApiContext>();

        builder.Services.AddScoped<RegisterController>();
        builder.Services.AddScoped<LoginController>();
        builder.Services.AddScoped<ErrorsController>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<TokenService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}

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

public class ErrorHandlingMiddleware(RequestDelegate @delegate, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await @delegate(context);
        }
        catch (Exception e)
        {
            var message = e.Message;
            log.LogError(e, message);
            await HandleExceptionAsync(context, e);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (status, message) = exception switch
        {
            _ => (HttpStatusCode.InternalServerError, "O, cholera! :((")
        };

        var errorMsg = JsonSerializer.Serialize(new { errorMessage = message });
        response.StatusCode = (int)status;
        return response.WriteAsync(errorMsg);
    }
}

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

public class UserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class TokenRequest
{
    public string RefreshToken { get; set; }
}

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

public partial class ApiContext : DbContext
{
    public virtual DbSet<AppUser> Users { get; set; }
    // public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public ApiContext()
    {
    }

    public ApiContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=APBD_11;User=sa;Password=fY0urP@sswor_Policy;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(user =>
            {
                user.HasKey(u => u.IdUser);
                user.Property(u => u.Username).IsRequired().HasMaxLength(100);
                user.Property(u => u.Password).IsRequired().HasMaxLength(100);
                user.Property(u => u.RefreshToken).IsRequired().HasMaxLength(100);
            }
        );
    }
}