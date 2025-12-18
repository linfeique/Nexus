using System.Security.Claims;
using System.Text;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nexus.Api.Auth;
using Nexus.Infrastructure.Abstractions;

namespace Nexus.Api.Features.Users.Cases;

public record LoginUseCaseRequest(
    string Email,
    string Password) : ICommand;

public record LoginUseCaseResponse(
    string AccessToken);

public class LoginUseCase(
    UserManager<User> userManager,
    IConfiguration configuration) : IHandler<LoginUseCaseRequest, Result<LoginUseCaseResponse>>
{
    public async Task<Result<LoginUseCaseResponse>> Handle(
        LoginUseCaseRequest request,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null ||
            await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result.Unauthorized();
        }
        
        var roles = await userManager.GetRolesAsync(user);
        
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = 
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("RestaurantId", user.RestaurantId.ToString()),
            ..roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };
        var tokenHandler = new JsonWebTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return new LoginUseCaseResponse(token);
    }
}