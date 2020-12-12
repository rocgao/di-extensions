using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SomeTest
{
    public class ParameterizedServiceResolver<T>
    {
        private class Constructor
        {
            private readonly string[] parameterNames;
            private readonly Func<IDictionary<string, object>, object> func;

            public Constructor(ConstructorInfo c)
            {
                this.parameterNames = c.GetParameters().Select(p => p.Name).ToArray();
                this.func = BuildMethod(c);
            }

            static Func<IDictionary<string, object>, object> BuildMethod(ConstructorInfo c)
            {
                var dictType = typeof(IDictionary<string, object>);
                var itemMethod = dictType.GetMethod("get_Item", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

                var paramExpr = Expression.Parameter(dictType, "parameters");
                var argExprs = new List<Expression>();
                foreach (var p in c.GetParameters())
                {
                    var expr = Expression.Convert(Expression.Call(paramExpr, itemMethod, Expression.Constant(p.Name)), p.ParameterType);
                    argExprs.Add(expr);
                }
                var newExpr = Expression.New(c, argExprs);
                var lambda = Expression.Lambda(newExpr, paramExpr);
                return lambda.Compile() as Func<IDictionary<string, object>, object>;
            }

            public bool Match(ICollection<string> targetParameterNames)
            {
                if (targetParameterNames.Count != this.parameterNames.Length)
                {
                    return false;
                }
                return !this.parameterNames.Except(targetParameterNames).Any();
            }

            public object New(IDictionary<string, object> parameters)
            {
                return func(parameters);
            }
        }

        private readonly Type impl;
        private readonly Constructor[] constructors;

        public ParameterizedServiceResolver(Type impl)
        {
            this.impl = impl;
            this.constructors = impl.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Select(item => new Constructor(item)).ToArray();
        }

        public T Resolve(IServiceProvider serviceProvider, IDictionary<string, object> parameters)
        {
            var parameterNames = parameters.Keys;
            foreach (var c in this.constructors)
            {
                if (c.Match(parameterNames))
                {
                    return (T)c.New(parameters);
                }
            }

            return default(T);
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