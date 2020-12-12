using System;
using Pinyin4net;
using Pinyin4net.Format;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace SomeTest
{
    class Program
    {
        private static readonly HanyuPinyinOutputFormat _pyFormat = new HanyuPinyinOutputFormat()
        {
            ToneType = HanyuPinyinToneType.WITHOUT_TONE,
            CaseType = HanyuPinyinCaseType.UPPERCASE,
        };
        static void Main(string[] args)
        {
            // var py = PinyinHelper.ToHanyuPinyinStringArray('f', _pyFormat);
            // var f = py.FirstOrDefault()?.FirstOrDefault();

            // var namedFoos = new Dictionary<string, Type>()
            // {
            //     {"A",typeof(FooA)},
            //     {"B",typeof(FooB)},
            // };

            // var namedBoos = new Dictionary<string, Type>()
            // {
            //     {"A1",typeof(BooA)},
            //     {"B1",typeof(BooB)},
            // };
            // var sp = new ServiceCollection()
            //     .AddNamedService<IFoo>(ServiceLifetime.Transient, namedFoos, typeof(FooC))
            //     .AddNamedService<IBoo>(ServiceLifetime.Singleton, namedBoos)
            //     .BuildServiceProvider();
            // for (int i = 0; i < 2; i++)
            // {
            //     var fooB = sp.GetNamedService<IFoo>("B");
            //     var fooA = sp.GetNamedService<IFoo>("A");
            //     var fooDef = sp.GetNamedService<IFoo>("");

            //     var booB = sp.GetNamedService<IBoo>("B1");
            //     var booA = sp.GetNamedService<IBoo>("A1");
            //     var booDef = sp.GetNamedService<IBoo>("C1");
            // }

            var sp = new ServiceCollection()
                .AddParameterizedService<IFoo, ParameterizedFoo>()
                .BuildServiceProvider();
            var foo = sp.GetParameterizedService<IFoo>(new Dictionary<string, object>(){
                {"name","gaopeng"},
                {"age",182},
                // {"attrs",new string[]{"a1","b2"}}
            });

            var foo3 = sp.GetParameterizedService<IFoo>(new Dictionary<string, object>(){
                {"name","gaopeng1"},
                {"age",13},
                {"attrs",new string[]{"c1","c2"}}
            });
        }
    }

    public interface IFoo
    {

    }

    public class FooA : IFoo
    {
        public FooA()
        {
            Console.WriteLine("new FooA");
        }
    }

    public class FooB : IFoo
    {
        public FooB()
        {
            Console.WriteLine("new FooB");
        }
    }
    public class FooC : IFoo
    {
        public FooC()
        {
            Console.WriteLine("new FooC");
        }
    }

    public interface IBoo
    {

    }

    public class BooA : IBoo
    {
        public BooA()
        {
            Console.WriteLine("new BooA");
        }
    }

    public class BooB : IBoo
    {
        public BooB()
        {
            Console.WriteLine("new BooB");
        }
    }
    public class BooC : IBoo
    {
        public BooC()
        {
            Console.WriteLine("new BooC");
        }
    }

    public class ParameterizedFoo : IFoo
    {
        private readonly string name;
        private readonly int age;
        private readonly string[] attrs;

        public ParameterizedFoo(string name, int age, string[] attrs)
        {
            this.name = name;
            this.age = age;
            this.attrs = attrs;
        }

        public ParameterizedFoo(string name, int age)
        {
            this.name = name;
            this.age = age;
        }
    }
}