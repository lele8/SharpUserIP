using System;
using System.Linq;

namespace SharpUserIP
{
    internal class Program
    {
        static void banner()
        {
            Console.WriteLine(@"
  ___ _                  _   _            ___ ___ 
 / __| |_  __ _ _ _ _ __| | | |___ ___ _ |_ _| _ \
 \__ \ ' \/ _` | '_| '_ \ |_| (_-</ -_) '_| ||  _/
 |___/_||_\__,_|_| | .__/\___//__/\___|_||___|_|  
                   |_|                                                                                       
");
        }
        static void Main(string[] args)
        {

            
            banner();
            string argDc = "";
            string argUser = "";
            string argPass = "";
            string argDay = "";
            string argGuser = "";
            string argFile = "";
            bool argAll = false;
            foreach (var entry in args.Select((value, index) => new { index, value }))
            {
                string argument = entry.value.ToUpper();

                switch (argument)
                {
                    case "-H":
                    case "/H":
                        argDc = args[entry.index + 1];
                        break;

                    case "-U":
                    case "/U":
                        argUser = args[entry.index + 1];
                        break;

                    case "-P":
                    case "/P":
                        argPass = args[entry.index + 1];
                        break;
                    case "-D":
                    case "/D":
                        argDay = args[entry.index + 1];
                        break;
                    case "-F":
                    case "/F":
                        argGuser = args[entry.index + 1];
                        break;
                    case "-ALL":
                    case "/ALL":
                        argAll = true;
                        break;
                    case "-O":
                    case "/O":
                        argFile = args[entry.index + 1];
                        break;
                }
            }
            if (args == null || !args.Any() || (args.Length<1 && args[0].Equals("h")))
            {

                Console.WriteLine(@"
Get the log of successful login or the specified user
By @lele

  -H             Specify the machine IP or machine name
  -U             Administrator account name
  -P             Administrator account password
  -D             Specify the time range (days) for enumeration
  -F             Enumerate specified users
  -O             Path to save the result, by default save to the log.txt of the current path
  -All           Get logs from all domain controllers

  Usage: 
       SharpUserIP.exe -d 7
       SharpUserIP.exe -h ip -d 7
       SharpUserIP.exe -h ip -d 7 -grep user
       SharpUserIP.exe -h ip -u username -p password -d 7
       SharpUserIP.exe -h ip -u username -p password -d 7 -all
       SharpUserIP.exe -h ip -u username -p password -d 7 -f user -o C:\path\result.txt
");
            }
            else if (!(string.IsNullOrEmpty(argUser) && string.IsNullOrEmpty(argPass)))
            {
                Simulation.Run(argDc, argUser, argPass, () =>
                {
                    GetLog.QueryLog(int.Parse(argDay), argAll, argDc, argGuser, argFile, argUser, argPass);
                });
            }
            else
            {
                GetLog.QueryLog(int.Parse(argDay), argAll, argDc, argGuser,argFile);
            }
        }

    }
}
