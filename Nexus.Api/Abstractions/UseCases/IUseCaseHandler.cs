namespace Nexus.Api.Abstractions.UseCases;

public interface IUseCaseHandler<in TRequest, TResponse> where TRequest : class, IUseCaseCommand
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct = default);
}