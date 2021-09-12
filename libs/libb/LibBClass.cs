using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace libbnamespace
{

    public class SomeClassFromB
    {
        static int num = 123;
        public void Xaxa(string st)
        {
        }
        public static int Call2(string a, string b)
        {
            Console.WriteLine("LibB call2");
            num += a.Length;
            return a.Length + b.Length;
        }
        public static int Call2(string a, string b,string c)
        {
            Console.WriteLine("LibB call2-3");
            num += a.Length;
            return a.Length + b.Length + c.Length;
        }
    }
    public class LibBClass
    {
        public static void LibBCall()
        {
            Console.WriteLine("LibBCall");
        }
    }
}
