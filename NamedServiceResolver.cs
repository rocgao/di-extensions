using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SomeTest
{
    public class NamedServiceResolver<T>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDictionary<string, Type> impls;
        private readonly Type defaultImpl;

        public NamedServiceResolver(IServiceProvider serviceProvider, IDictionary<string, Type> impls, Type defaultImpl)
        {
            this.serviceProvider = serviceProvider;
            this.impls = impls;
            this.defaultImpl = defaultImpl;
        }

        public T Resolve(string name)
        {
            if (impls.TryGetValue(name, out var v))
            {
                return (T)serviceProvider.GetService(v);
            }

            if (defaultImpl == null)
            {
                return default(T);
            }
            return (T)serviceProvider.GetService(defaultImpl);
        }
    }

    public static class NamedServiceResolverExtensions
    {
        public static IServiceCollection AddNamedService<T>(this IServiceCollection services, ServiceLifetime lifetime, IDictionary<string, Type> impls, Type defaultImpl = null)
        {
            foreach (var impl in impls)
            {
                var descriptor = new ServiceDescriptor(impl.Value, impl.Value, lifetime);
                services.TryAdd(descriptor);
            }

            if (defaultImpl != null)
            {
                services.TryAdd(new ServiceDescriptor(defaultImpl, defaultImpl, lifetime));
            }

            var tDescriptor = new ServiceDescriptor(typeof(NamedServiceResolver<T>), p => new NamedServiceResolver<T>(p, impls, defaultImpl), ServiceLifetime.Singleton);
            services.Add(tDescriptor);

            return services;
        }
        public static T GetNamedService<T>(this IServiceProvider serviceProvider, string name)
        {
            var namedServiceResolver = serviceProvider.GetService<NamedServiceResolver<T>>();
            return namedServiceResolver.Resolve(name);
        }
    }
}