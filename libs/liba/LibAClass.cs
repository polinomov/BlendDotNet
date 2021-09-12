using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libbnamespace;

namespace liba
{
    class AnotherLibA
    {
        class NestedA
        {
            int a, b, c;
        }
        static public void MethodFroMyLib(int val)
        {
            Console.WriteLine("LIBA- MethodA");
        }
    }

    public class LibAClass
    {
        static int num = 123;
        int Call2(string a, string b)
        {
            num += a.Length;
            return a.Length + b.Length;
        }
        public static void LibACall()
        {
            AnotherLibA.MethodFroMyLib(123);
            int r1 = SomeClassFromB.Call2("Hello", "World");
            int r2 =  SomeClassFromB.Call2("Hello", "World", "Blin");
            Console.WriteLine("r1=" + r1 + " r2=" + r2);
        }
    }
}
