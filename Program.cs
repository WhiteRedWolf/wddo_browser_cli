using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net;

namespace WDDO_Browser_cli {
    class Program {

        public static string PostRequest(string url, NameValueCollection parameters)
        {
            using(WebClient client = new WebClient())
            {
                byte[] arr = client.UploadValues(url, "POST", parameters);
                //File.WriteAllBytes("wtf", arr);
                return Encoding.UTF8.GetString(arr);
            }
        }


        static void Main(string[] args)
        {
            if (!File.Exists("general.cfg"))
            {
                File.WriteAllLines("general.cfg", new string[] {
                    "{",
                    "\t\"version\": 1,",
                    "\t\"auto_update\": true,",
                    "\t\"default_homepage\": \"wddo@home\"",
                    "}"
                });

                File.WriteAllLines("dnssettings.cfg", new string[] {
                    "{",
                    "\t\"dns_actionmethod\": \"post\",",
                    "\t\"dns_adress\": \"wddo.6te.net\",",
                    "\t\"dns_contentversion\": \"original\",",
                    "\t\"dns_password\": \"\"",
                    "}"
                });
            }
            if(args.Length != 0)
            {
                if(args[0] == "host")
                {
                    NameValueCollection data = new NameValueCollection();
                    data["requesttype"] = "host_dns";
                    data["hostname"] = args[1];
                    try
                    {
                        data["hostpw"] = args[3];
                    }catch
                    {
                        data["hostpw"] = "";
                    }
                    data["httplocation"] = args[2];
                    string r = PostRequest("http://wddo.6te.net/dns-host.php", data);
                    Console.WriteLine(r);
                    Environment.Exit(0);
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" 001 Loading DNS Configuration");
            //Set Default DNS
            ObjectNotations.DNS DNSSettings = JsonConvert.DeserializeObject<ObjectNotations.DNS>(File.ReadAllText("dnssettings.cfg"));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" 200 OK DNS Successfully Loaded!");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" 002 Sending POST Request for 'response_test' on {0}", DNSSettings.dns_adress);

            //Check if DNS Responds

            var response_test_data = new NameValueCollection();
            response_test_data["password"] = DNSSettings.dns_password;
            response_test_data["requesttype"] = "response_test";

            string response_test = PostRequest("http://wddo.6te.net/dns-request.php", response_test_data);

            if (response_test == "200 OK")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("%100 Recieved Response by Server: { \"200 OK\" } ");
            } else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("%101 Recieved unhandled Response by Server: { \"" + response_test + "\" }");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("     Press ENTER to continue anyway...");
                Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("%103 Requesting Location of Homepage..");

            var wddo_homepage_data = new NameValueCollection();
            wddo_homepage_data["password"] = DNSSettings.dns_password;
            wddo_homepage_data["requesttype"] = "location_get";
            wddo_homepage_data["request_location"] = "wddo@home";

            string get_wddo_homepage = PostRequest("http://wddo.6te.net/dns-request.php", wddo_homepage_data);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("%100 Recieved Location of Homepage from Server");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("%103 Requesting Contents of Homepage");

            var ping_homepage_data = new NameValueCollection();
            ping_homepage_data["request"] = "wddo";

            var ping_homepage = PostRequest(get_wddo_homepage.Split(' ')[1] + "/default.php", ping_homepage_data);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("%100 Recieved Contents of Homepage. Displaying now::");

            Console.WriteLine("##############################################################################################################");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ping_homepage);
            Console.ReadLine();
        }
    }
}
