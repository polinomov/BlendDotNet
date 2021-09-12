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
            dllList.Add(@"C:\Dev\BlendDotNet\libs\libb\bin\Debug\net472\libb.dll");
            string prime  = @"C:\Dev\BlendDotNet\libs\liba\bin\Debug\net472\liba.dll";
            string result = @"C:\Dev\BlendDotNet\result\liba.dll";
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
