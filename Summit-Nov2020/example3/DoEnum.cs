// DoEnum.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;


namespace SharpEnumLibrary
{
    public class DoEnum
    {
        public static string GetUsers()
        {
            var output = "Local users...\n";

            SelectQuery query = new SelectQuery("Win32_UserAccount");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject user in searcher.Get())
            {
                output += user["Name"] +"\n";
            }

            return output;
        }

        public static string GetRunningProcesses()
        {
            var output = "Running processes...\n";

            Process[] processlist = Process.GetProcesses();

            output += "PID\tName\n";
            foreach (Process p in processlist.OrderBy(m => m.ProcessName))
            {
                output += p.Id +"\t"+ p.ProcessName +"\n";
            }

            return output;
        }

    }
}
