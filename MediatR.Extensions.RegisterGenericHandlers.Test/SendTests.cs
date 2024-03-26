using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Extensions.RegisterGenericHandlers.Test
{
    [TestClass]
    public class SendTests
    {
        private IServiceProvider? _serviceProvider;
        private IMediator? _mediator;

        [TestInitialize]
        public void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Foo>())
                .RegisterGenericHandlersFromAssemblyConataining<Foo>();
            _serviceProvider = services.BuildServiceProvider();

            _mediator = _serviceProvider.GetService<IMediator>();
        }

        [TestMethod]
        public async Task Should_resolve_GenericRequestHandler()
        {   
            var request = new GenericRequest<Foo>
            {
                Foo = new()
            };

            var result = await _mediator!.Send(request);
            var expected = typeof(GenericRequestHandler<Foo>).FullName;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public async Task Should_resolve_GenericRequest2Handler()
        {
            var request = new GenericRequest2<Foo>
            {
                Foo = new()
            };

            var result = await _mediator!.Send(request);
            var expected = typeof(GenericRequest2Handler<Foo>).FullName;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public async Task Should_resolve_GenericVoidRequestHandler()
        {
            var request = new GenericVoidRequest<Foo>
            {
                Foo = new()
            };

            await _mediator!.Send(request);
            var expected = typeof(GenericVoidRequestHandler<Foo>).FullName;

            Assert.AreEqual(expected, request.Foo.Bar);
        }

        [TestMethod]
        public async Task Should_resolve_GenericVoidRequest2Handler()
        {
            var request = new GenericVoidRequest2<Foo>
            {
                Foo = new()
            };

            await _mediator!.Send(request);
            var expected = typeof(GenericVoidRequest2Handler<Foo>).FullName;

            Assert.AreEqual(expected, request.Foo.Bar);
        }
    }
}
