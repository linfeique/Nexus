namespace Nexus.Infrastructure.Abstractions;

public interface IHandler<in TRequest, TResponse> where TRequest : class, ICommand
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct = default);
}