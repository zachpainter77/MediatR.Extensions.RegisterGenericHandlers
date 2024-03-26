using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.RegisterGenericHandlers.Test
{
    [TestClass]
    public class AssemblyResolution
    {
        private IServiceProvider? _serviceProvider;

        [TestInitialize]
        public void Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Foo>())
                .RegisterGenericHandlersFromAssemblyConataining<Foo>();
            _serviceProvider = services.BuildServiceProvider();

        }

        [TestMethod]
        public void Should_resolve_GenericRequestHandler()
        {
            var service = _serviceProvider!.GetService<IRequestHandler<GenericRequest<Foo>, string>>();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Should_resolve_GenericRequest2Handler()
        {
            var service = _serviceProvider!.GetService<IRequestHandler<GenericRequest2<Foo>, string>>();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Should_resolve_GenericVoidRequestHandler()
        {
            var service = _serviceProvider!.GetService<IRequestHandler<GenericVoidRequest<Foo>>>();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Should_resolve_GenericVoidRequest2Handler()
        {
            var service = _serviceProvider!.GetService<IRequestHandler<GenericVoidRequest2<Foo>>>();
            Assert.IsNotNull(service);
        }
    }
}
