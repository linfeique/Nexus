using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Nexus.Api.Abstractions;
using Nexus.Api.Abstractions.UseCases;
using Nexus.Api.Auth;

namespace Nexus.Api.Features.Users.Cases;

public record RegisterUseCaseRequest(
    string Email,
    string Password) : IUseCaseCommand;

public record RegisterUseCaseResponse(
    Guid UserId,
    string Email,
    Guid RestaurantId);

public class RegisterUseCase(
    AuthDbContext dbContext,
    UserManager<User> userManager) : IUseCaseHandler<RegisterUseCaseRequest, Result<RegisterUseCaseResponse>>
{
    public async Task<Result<RegisterUseCaseResponse>> Handle(
        RegisterUseCaseRequest request,
        CancellationToken ct = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            RestaurantId = Guid.CreateVersion7()
        };
        
        var createUserResult = await userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
            return Result.Invalid(createUserResult.Errors.Select(e => new ValidationError(e.Code, e.Description)));

        var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.User);
        if (!addToRoleResult.Succeeded)
            return Result.Invalid(addToRoleResult.Errors.Select(e => new ValidationError(e.Code, e.Description)));
        
        await transaction.CommitAsync(ct);
        
        var response = new RegisterUseCaseResponse(
            new Guid(user.Id),
            user.Email, 
            user.RestaurantId);

        return response;
    }
}