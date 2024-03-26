using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Extensions.RegisterGenericHandlers.Test
{
    public interface IFoo
    {
        string Bar { get; set; }        
    }

    public class Foo : IFoo
    {
        public string Bar { get; set; } = "Foo Bar";       
    }

    public class GenericRequest<T> : IRequest<string>
        where T : class, IFoo
    {
        public T? Foo { get; set; }
    }

    public class GenericRequestHandler<T> : IRequestHandler<GenericRequest<T>, string?>
        where T : class, IFoo
    {
        public Task<string?> Handle(GenericRequest<T> request, CancellationToken cancellationToken)
        {
            request.Foo!.Bar = GetType().FullName!;

            return Task.FromResult(request.Foo?.Bar);
        }
    }

    public class GenericRequest2<T> : IRequest<string>
       where T : class, IFoo
    {
        public T? Foo { get; set; }
    }

    public class GenericRequest2Handler<T> : IRequestHandler<GenericRequest2<T>, string?>
        where T : class, IFoo
    {
        public Task<string?> Handle(GenericRequest2<T> request, CancellationToken cancellationToken)
        {
            request.Foo!.Bar = GetType().FullName!;
            return Task.FromResult(request.Foo?.Bar);
        }
    }

    public class GenericVoidRequest<T> : IRequest
       where T : class, IFoo
    {
        public T? Foo { get; set; }
    }

    public class GenericVoidRequestHandler<T> : IRequestHandler<GenericVoidRequest<T>>
        where T : class, IFoo
    {
        public Task Handle(GenericVoidRequest<T> request, CancellationToken cancellationToken)
        {
            request.Foo!.Bar = GetType().FullName!;
            return Task.CompletedTask;
        }
    }

    public class GenericVoidRequest2<T> : IRequest
       where T : class, IFoo
    {
        public T? Foo { get; set; }
    }

    public class GenericVoidRequest2Handler<T> : IRequestHandler<GenericVoidRequest2<T>>
        where T : class, IFoo
    {
        public Task Handle(GenericVoidRequest2<T> request, CancellationToken cancellationToken)
        {
            request.Foo!.Bar = GetType().FullName!;
            return Task.CompletedTask;
        }
    }
}
