MediatR.Extensions.RegisterGenericHandlers
===========================================

![CI](https://github.com/zachpainter77/MediatR.Extensions.RegisterGenericHandlers/actions/workflows/BuildAndTest.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/dt/MediatR.Extensions.RegisterGenericHandlers)](https://www.nuget.org/packages/mediatr.extensions.registergenerichandlers) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatR.Extensions.RegisterGenericHandlers)](https://www.nuget.org/packages/mediatr.extensions.registergenerichandlers)

Simple extension method for MediatR to register open generic request handlers.

Created as supplement to the famous MediatR library. (MediatR is the only Dependency).

### Installing MediatR.Extensions.RegisterGenericHandlers

You should install [MediatR.Extensions.RegisterGenericHandlers with NuGet](https://www.nuget.org/packages/MediatR.Extensions.RegisterGenericHandlers):

    Install-Package MediatR.Extensions.RegisterGenericHandlers
    
Or via the .NET Core command line interface:

    dotnet add package MediatR.Extensions.RegisterGenericHandlers

Either commands, from Package Manager Console or .NET Core CLI, will download and install MediatR.Extensions.RegisterGenericHandlers and all required dependencies.

### Registering with `IServiceCollection`

Extension method should be called on the service collection in your Program.cs file. I typically do it immediatlely after the MediatR registration.

```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>())
    .RegisterGenericHandlersFromAssemblyContaining<Startup>();
```

or with an `IEnumerable<Assembly>` of assemblies to scan:

```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly))
    .RegisterGenericHandlers([typeof(Startup).Assembly]);
```

This registers open generic implementations for:

- `IRequestHandler<,>` open implementations as transient
- `IRequestHandler<>` open implementations as transient

### Example Handler

Below is an example open generic request / handler implementation.

Some concrete class (`Foo`) that implements an interface (`IFoo`). This will be used as a type parameter for the example.

```csharp
    public interface IFoo
    {
        string Bar { get; set; }        
    }

    public class Foo : IFoo
    {
        public string Bar { get; set; } = "Foo Bar";       
    }
```

Open request implementation that constrains the type parameter to `IFoo`

```csharp

    public class GenericRequest<T> : IRequest<string>
        where T : class, IFoo
    {
        public T? Foo { get; set; }
    }
```

Open handler implementation for `GenericRequest<T>`

```csharp

    public class GenericRequestHandler<T> : IRequestHandler<GenericRequest<T>, string?>
        where T : class, IFoo
    {
        public Task<string?> Handle(GenericRequest<T> request, CancellationToken cancellationToken)
        {
            request.Foo!.Bar = "Hello From Handler!!";

            return Task.FromResult(request.Foo?.Bar);
        }
    }
```

 Usage from MediatR Send method.

 ```csharp
    var request = new GenericRequest<Foo>
    {
        Foo = new()
    };
    var result = await _mediator.Send(request);

    Console.WriteLine(result); //outputs "Hello From Handler!!"
 ```
 

