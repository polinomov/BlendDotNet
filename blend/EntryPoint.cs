using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blend
{
    class EntryPoint
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Blend");
            List<string> dllList = new List<string>();
            //dllList.Add(@"C:\Dev\BlendDotNet\libs\libb\bin\Debug\net472\libb.dll");
            //string prime  = @"C:\Dev\BlendDotNet\libs\liba\bin\Debug\net472\liba.dll";
            string prime = args[0];
            dllList.Add(args[1]);
            string result = args[2];
            try
            {
                Mixer.DoMix(prime, dllList, result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception " + e.Message);
            }
        }
    }
}
