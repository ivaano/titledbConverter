using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace titledbConverter.Infrastructure;

public sealed class TypeResolver(IHost provider) : ITypeResolver, IDisposable
{
    private readonly IHost _host = provider ?? throw new ArgumentNullException(nameof(provider));

    public object? Resolve(Type? type)
    {
        return type != null ? _host.Services.GetService(type) : null;
    }

    public void Dispose()
    {
        _host.Dispose();
    }
}