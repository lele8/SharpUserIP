using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpUserIP
{
    internal class GetLog
    {
        public string UserName;
        public string DomainName;
        public string IP;
        public string Sid;
        public static List<GetLog> Loginfo { get; set; } = new List<GetLog>();
        public static string[] attributes = null;
        public static List<string> GetDC(string dc, string LogoinUser = null, string LoginPass = null)
        {
            List<string> list = new List<string>();
            string DomainName = dc ?? Environment.GetEnvironmentVariable("USERDNSDOMAIN");
            DirectoryEntry directoryEntry = new DirectoryEntry($"LDAP://{DomainName}:389", LogoinUser, LoginPass, AuthenticationTypes.Secure);

            DirectorySearcher searcher = new DirectorySearcher(directoryEntry);
            searcher.Filter = "(&(objectCategory=computer)(objectClass=computer)(userAccountControl:1.2.840.113556.1.4.803:=8192))";
            foreach (SearchResult read in searcher.FindAll())
            {
                string Dc = read.Properties["name"][0].ToString();
                list.Add(Dc);
            }
            return list;
        }

        public static void QueryLog(int days, bool all, string dc = null, string user = null, string outFile = null,string LogoinUser = null,string LoginPass = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                    outFile = Directory.GetCurrentDirectory() + "\\log.txt";
                string outFilePath = Path.GetFullPath(outFile);
                if (!string.IsNullOrEmpty(user)) { attributes = user.Split(','); }
                if (all)
                {
                    foreach (string dcs in GetDC(dc, LogoinUser, LoginPass))
                    {
                        EventLog alllog = new EventLog("Security", dcs);
                        var allentries = alllog.Entries.Cast<EventLogEntry>().Where(x => (x.InstanceId == 4624 && x.TimeGenerated >= DateTime.Now.AddDays(0 - days)));
                        foreach (EventLogEntry logs in allentries)
                        {
                            string sid = logs.ReplacementStrings[4];
                            string username = logs.ReplacementStrings[5];
                            string sip = logs.ReplacementStrings[18];
                            string domainname = logs.ReplacementStrings[6];

                            if (sid.Length > 12 && (!username.Contains("$")) && (sip.Length > 1))
                            {
                                if (!Loginfo.Exists(x => (x.Sid == logs.ReplacementStrings[4]) && (x.IP == logs.ReplacementStrings[18])))
                                {
                                    Loginfo.Add(new GetLog()
                                    {
                                        Sid = sid,
                                        IP = sip,
                                        UserName = username,
                                        DomainName = domainname
                                    });
                                }
                            }
                        }
                        alllog.Close();
                    }
                }
                else if (string.IsNullOrEmpty(dc))
                {
                    EventLog log = new EventLog("Security");
                    var entries = log.Entries.Cast<EventLogEntry>().Where(x => (x.InstanceId == 4624 && x.TimeGenerated >= DateTime.Now.AddDays(0 - days)));
                    foreach (EventLogEntry logs in entries)
                    {
                        string sid = logs.ReplacementStrings[4];
                        string username = logs.ReplacementStrings[5];
                        string sip = logs.ReplacementStrings[18];
                        string domainname = logs.ReplacementStrings[6];

                        if (sid.Length > 12 && (!username.Contains("$")) && (sip.Length > 1))
                        {
                            if (!Loginfo.Exists(x => (x.Sid == logs.ReplacementStrings[4]) && (x.IP == logs.ReplacementStrings[18])))
                            {
                                Loginfo.Add(new GetLog()
                                {
                                    Sid = sid,
                                    IP = sip,
                                    UserName = username,
                                    DomainName = domainname
                                });
                            }
                        }
                    }
                    log.Close();
                }
                else
                {
                    EventLog log = new EventLog("Security", dc);
                    var entries = log.Entries.Cast<EventLogEntry>().Where(x => (x.InstanceId == 4624 && x.TimeGenerated >= DateTime.Now.AddDays(0 - days)));
                    foreach (EventLogEntry logs in entries)
                    {
                        string sid = logs.ReplacementStrings[4];
                        string username = logs.ReplacementStrings[5];
                        string sip = logs.ReplacementStrings[18];
                        string domainname = logs.ReplacementStrings[6];

                        if (sid.Length > 12 && (!username.Contains("$")) && (sip.Length > 1))
                        {
                            if (!Loginfo.Exists(x => (x.Sid == logs.ReplacementStrings[4]) && (x.IP == logs.ReplacementStrings[18])))
                            {
                                Loginfo.Add(new GetLog()
                                {
                                    Sid = sid,
                                    IP = sip,
                                    UserName = username,
                                    DomainName = domainname
                                });
                            }
                        }
                    }
                    log.Close();
                }
                if (attributes != null)
                {
                    foreach (var u in attributes)
                    {
                        var Find = from f in Loginfo where f.UserName == u select f;
                        foreach (var F in Find)
                        {
                            foreach (var f in typeof(GetLog).GetFields(BindingFlags.Public | BindingFlags.Instance))
                            {
                                Console.WriteLine("    {0, -25}  {1,-3}", f.Name, f.GetValue(F));
                                File.AppendAllText(outFilePath, $"    {f.Name,-25}  {f.GetValue(F),-3}" + Environment.NewLine);
                            }
                            Console.WriteLine();
                            File.AppendAllText(outFilePath, Environment.NewLine);
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    foreach (var log in Loginfo)
                    {
                        foreach (var f in typeof(GetLog).GetFields(BindingFlags.Public | BindingFlags.Instance))
                        {
                            Console.WriteLine("    {0, -25}  {1,-3}", f.Name, f.GetValue(log));
                            File.AppendAllText(outFilePath, $"    {f.Name,-25}  {f.GetValue(log),-3}" + Environment.NewLine);
                        }
                        Console.WriteLine();
                        File.AppendAllText(outFilePath, Environment.NewLine);
                    }
                    Console.WriteLine();
                }
            }
            catch { }
        }
    }
}
