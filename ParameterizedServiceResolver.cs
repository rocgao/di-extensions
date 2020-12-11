using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SomeTest
{
    public class ParameterizedServiceResolver<T>
    {
        private readonly Type impl;
        private readonly ConstructorInfo[] constructors;

        public ParameterizedServiceResolver(Type impl)
        {
            this.impl = impl;
            this.constructors = impl.GetConstructors(BindingFlags.Public | BindingFlags.CreateInstance);
        }


        public T Resolve(IServiceProvider serviceProvider, IDictionary<string, object> parameters)
        {
            var constructor = SelectConstructor(parameters);

            // foreach (var p in constructor.GetParameters())
            // {
            //     p.Name
            // }
            // constructor.Invoke()
            return default(T);
        }

        private ConstructorInfo SelectConstructor(IDictionary<string, object> parameters)
        {
            if (constructors.Length == 1)
            {
                return constructors[0];
            }
            var paramCounter = parameters.Count;
            return constructors.FirstOrDefault(m => m.GetParameters().Length == paramCounter) ?? throw new InvalidOperationException("未找到适合的构造函数");
        }
    }

    public static class ParameterizedServiceResolverExtensions
    {
        public static IServiceCollection AddParameterizedService<TService, TImpl>(this IServiceCollection services)
        {
            services.AddSingleton<ParameterizedServiceResolver<TService>>(new ParameterizedServiceResolver<TService>(typeof(TImpl)));
            return services;
        }

        public static TService GetParameterizedService<TService>(this IServiceProvider serviceProvider, IDictionary<string, object> parameters)
        {
            var resolver = serviceProvider.GetService<ParameterizedServiceResolver<TService>>();
            return resolver.Resolve(serviceProvider, parameters);
        }
    }
}