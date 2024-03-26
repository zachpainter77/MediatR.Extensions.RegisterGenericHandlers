using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;

namespace MediatR.Extensions.RegisterGenericHandlers
{
    /// <summary>
    /// static class to hold extension methods
    /// </summary>
    public static class RegisterGenericHandlers
    {
        /// <summary>
        /// Extension Method to register generic handlers from assemblies containing type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="typeEvaluator"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterGenericHandlersFromAssemblyConataining<T>(this IServiceCollection services, Func<Type, bool>? typeEvaluator = null)
        {
            var assemblies = typeof(T).Assembly;
            return RegisterGenericMediatorHandlers(services, [assemblies], typeEvaluator);
        }

        /// <summary>
        /// Extension Method to register generic handlers
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembliesToScan"></param>
        /// <param name="typeEvaluator"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterGenericMediatorHandlers(this IServiceCollection services, IEnumerable<Assembly> assembliesToScan, Func<Type, bool>? typeEvaluator = null)
        {
            typeEvaluator ??= t => true;

            var openRequestHandlerInterfaces = new Type[] {
                    typeof(IRequestHandler<>),
                    typeof(IRequestHandler<,>)
            };

            foreach (var openRequestHandlerInterface in openRequestHandlerInterfaces)
            {

                var genericHandlers = assembliesToScan
                    .SelectMany(assembly => assembly.DefinedTypes)
                    .Where(type => type.IsConcrete() && type.IsOpenGeneric() && type.FindInterfacesThatClose(openRequestHandlerInterface).Any())
                    .Where(typeEvaluator)
                    .ToList();

                AddAllConcretionsThatClose(openRequestHandlerInterface, genericHandlers, services, assembliesToScan);
            }

            return services;
        }

        private static (Type Service, Type Implementation) GetConcreteRegistrationTypes(Type openRequestHandlerInterface, Type concreteGenericTRequest, Type openRequestHandlerImplementation)
        {
            var closingType = concreteGenericTRequest.GetGenericArguments().First();

            var concreteTResponse = concreteGenericTRequest.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequest<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

            var typeDefinition = openRequestHandlerInterface.GetGenericTypeDefinition();

            var serviceType = concreteTResponse != null ?
                typeDefinition.MakeGenericType(concreteGenericTRequest, concreteTResponse) :
                typeDefinition.MakeGenericType(concreteGenericTRequest);

            return (serviceType, openRequestHandlerImplementation.MakeGenericType(closingType));
        }

        private static List<Type>? GetConcreteRequestTypes(Type concreteInterface, Type openRequestHandlerImplementation, IEnumerable<Assembly> assembliesToScan)
        {
            var constraints = openRequestHandlerImplementation.GetGenericArguments().First().GetGenericParameterConstraints();   

            var typesThatCanClose = assembliesToScan
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && constraints.All(constraint => constraint.IsAssignableFrom(type)))
                .ToList();

            var requestType = concreteInterface.GenericTypeArguments.First();

            if (requestType.IsGenericParameter)
                return null;

            var requestGenericTypeDefinition = requestType.GetGenericTypeDefinition();

            return typesThatCanClose.Select(type => requestGenericTypeDefinition.MakeGenericType(type)).ToList();
        }

        private static void AddAllConcretionsThatClose(Type openRequestInterface, List<Type> concretions, IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            foreach (var concretion in concretions)
            {
                var concreteInterface = concretion
                    .GetInterfaces()
                    .FirstOrDefault(i => i.Name.StartsWith("IRequestHandler"));

                if (concreteInterface is null)
                    continue;

                var concreteRequests = GetConcreteRequestTypes(concreteInterface, concretion, assembliesToScan);

                if (concreteRequests is null)
                    continue;

                var registrationTypes = concreteRequests
                    .Select(concreteRequest => GetConcreteRegistrationTypes(openRequestInterface, concreteRequest, concretion));

                foreach (var (Service, Implementation) in registrationTypes)
                {
                    services.AddTransient(Service, Implementation);
                }
            }
        }

        internal static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null) return false;

            if (pluggedType == pluginType) return true;

            return pluginType.IsAssignableFrom(pluggedType);
        }

        private static bool IsOpenGeneric(this Type type)
        {
            return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
        }

        internal static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
        {
            if (pluggedType == null) yield break;

            if (!pluggedType.IsConcrete()) yield break;

            if (templateType.IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.BaseType!.IsGenericType &&
                     (pluggedType.BaseType!.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.BaseType!;
            }

            if (pluggedType.BaseType == typeof(object)) yield break;

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }
    }
}
