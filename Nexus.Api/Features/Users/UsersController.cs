using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Features.Users.Cases;

namespace Nexus.Api.Features.Users;

[ApiController]
[Route("api/[controller]")]
[TranslateResultToActionResult]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public Task<Result<RegisterUseCaseResponse>> Register(
        [FromServices] RegisterUseCase useCase,
        [FromBody] RegisterUseCaseRequest request,
        CancellationToken ct)
    {
        return useCase.Handle(request, ct);
    }
    
    [HttpPost("login")]
    public Task<Result<LoginUseCaseResponse>> Login(
        [FromServices] LoginUseCase useCase,
        [FromBody] LoginUseCaseRequest request,
        CancellationToken ct)
    {
        return useCase.Handle(request, ct);
    }
}