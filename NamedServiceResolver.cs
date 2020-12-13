using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SomeTest
{
    internal class NamedInstanceResolver<T>
    {
        private readonly IDictionary<string, object> instances;
        private readonly object defaultInstance;

        public NamedInstanceResolver(IDictionary<string, object> instances, object defaultInstance)
        {
            this.instances = instances;
            this.defaultInstance = defaultInstance;
        }

        public T Resolve(IServiceProvider serviceProvider, string name)
        {
            if (instances.TryGetValue(name, out var v))
            {
                return (T)v;
            }

            return (T)defaultInstance;
        }
    }
    internal class NamedServiceResolver<T>
    {
        private readonly IDictionary<string, Type> impls;
        private readonly Type defaultImpl;


        public NamedServiceResolver(IDictionary<string, Type> impls, Type defaultImpl)
        {
            this.impls = impls;
            this.defaultImpl = defaultImpl;
        }

        public T Resolve(IServiceProvider serviceProvider, string name)
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

            services.AddSingleton<NamedServiceResolver<T>>(new NamedServiceResolver<T>(impls, defaultImpl));

            return services;
        }

        public static IServiceCollection AddNamedInstance<T>(this IServiceCollection services, IDictionary<string, object> instances, object defaultInstance = null)
        {
            services.AddSingleton<NamedInstanceResolver<T>>(new NamedInstanceResolver<T>(instances, defaultInstance));
            return services;
        }
        public static T GetNamedService<T>(this IServiceProvider serviceProvider, string name)
        {
            var namedServiceResolver = serviceProvider.GetService<NamedServiceResolver<T>>();
            return namedServiceResolver.Resolve(serviceProvider, name);
        }
        public static T GetNamedInstance<T>(this IServiceProvider serviceProvider, string name)
        {
            var namedInstanceResolver = serviceProvider.GetService<NamedInstanceResolver<T>>();
            return namedInstanceResolver.Resolve(serviceProvider, name);
        }
    }
}