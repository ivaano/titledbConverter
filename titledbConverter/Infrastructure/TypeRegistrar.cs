using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace titledbConverter.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IHostBuilder _builder;

    public TypeRegistrar(IHostBuilder builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_builder.Build());
    }

    public void Register(Type service, Type implementation)
    {
        _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        if (factory is null) throw new ArgumentNullException(nameof(factory));

        _builder.ConfigureServices((_, services) => services.AddSingleton(service, _ => factory()));
    }
}