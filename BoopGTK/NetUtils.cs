using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BoopGTK
{
    public class NetUtils
    {
        //Known Nintendo Mac adresses
        public static readonly List<string> Nintendo = new List<string> { "DC68EB", "E84ECE", "E0E751", "E00C7F", "D86BF7", "CCFB65", "CC9E00", "B8AE6E", "A4C0E1", "A45C27", "9CE635", "98B6E9", "8CCDE8", "8C56C5", "7CBB8A", "78A2A0", "58BDA3", "40F407", "40D28A", "34AF2C", "2C10C1", "182A7B", "002709", "002659", "0025A0", "0024F3", "002444", "00241E", "0023CC", "002331", "0022D7", "0022AA", "00224C", "0021BD", "002147", "001FC5", "001F32", "001EA9", "001E35", "001DBC", "001CBE", "001BEA", "001B7A", "001AE9", "0019FD", "00191D", "0017AB", "001656", "0009BF" };

        public static object Get { get; private set; }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static string GetDSIP()
        {
            if (MainClass.IsUnix())
            {
                foreach (var item in GetIpPairsUnix())
                {
                    string MAC = item.MacAddress;
          
                    if (Nintendo.Contains(MAC))
                    {
                        Console.WriteLine("Found");
                        Console.WriteLine(item.MacAddress);
                        Console.WriteLine(item.IpAddress);
                        return item.IpAddress;
                    }
                }
            } else {
                foreach (var item in GetIPPairsWindows())
                {
                    string MAC = item.IpAddress;
                    if (Nintendo.Contains(MAC))
                    {
                        Console.WriteLine("Found");
                        Console.WriteLine(item.MacAddress);
                        Console.WriteLine(item.IpAddress);
                        return item.IpAddress;
                    }
                }
            }

            return "Not found";
        }
        public static List<MacIpPair> GetIPPairsWindows()
        {
            try
            {
                List<MacIpPair> list = new List<MacIpPair>();

                foreach (var arp in GetARPResult().Split(new char[] { '\n', '\r' }))
                {
                    // Parse out all the MAC / IP Address combinations
                    if (!string.IsNullOrEmpty(arp))
                    {
                        var pieces = (from piece in arp.Split(new char[] { ' ', '\t' })
                                      where !string.IsNullOrEmpty(piece)
                                      select piece).ToArray();
                        if (pieces.Length == 3)
                        {
                            //Clean the mac adress since we only need the first 6
                            string MAC = pieces[1];
                            MAC = MAC.Replace("-", "");
                            if (MAC.Length > 0)
                            {
                                MAC = MAC.Substring(0, 6);
                            }
                            MAC = MAC.ToUpper();

                            list.Add(new MacIpPair()
                            {
                                MacAddress = MAC,
                                IpAddress = pieces[0]
                            });
                        }
                    }
                }

                // Return list of IPInfo objects containing MAC / IP Address combinations
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("GetIPPairsWindows: Error Parsing 'arp -a' results", ex);
            }
        }
          
        public static List<MacIpPair> GetIpPairsUnix()
        {
            try
            {
                List<MacIpPair> pairs = new List<MacIpPair>();
                string arp = GetARPResult();

                Regex pattern = new Regex(@"(?<ip>([0-9]{1,3}\.?){4})\S\s\S+\s(?<mac>([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2}))");
                MatchCollection mc = pattern.Matches(arp);
                foreach (Match m in mc)
                {
                    //Clean the mac adress since we only need the first 6
                    string MAC = m.Groups["mac"].Value;
                    MAC = MAC.Replace(":", "");
                    if (MAC.Length > 0)
                    {
                        MAC = MAC.Substring(0, 6);
                    }
                    MAC = MAC.ToUpper();

                    Console.WriteLine("Pair Test");
                    Console.WriteLine(MAC);
                    Console.WriteLine(m.Groups["ip"].Value);

                    pairs.Add(new MacIpPair()
                    {
                        MacAddress = MAC,
                        IpAddress = m.Groups["ip"].Value
                    });

                }
                return pairs;
            }
            catch (Exception ex)
            {
                throw new Exception("GetIpPairsUnix: Error Parsing 'arp -a' results", ex);
            }
        }


        public struct MacIpPair
        {
            public string MacAddress;
            public string IpAddress;
        }



        /// <summary>
        /// http://pietschsoft.com/post/2009/11/08/Resolve_IP_Address_And_Host_Name_From_MAC_Address_using_CSharp_and_Windows_ARP_Utility
        /// All credit for this code goes to Chris Pietschmann
        /// This runs the "arp" utility in Windows to retrieve all the MAC / IP Address entries.
        /// </summary>
        /// <returns></returns>
        private static string GetARPResult()
        {
            Process p = null;
            string output = string.Empty;

            try
            {
                p = Process.Start(new ProcessStartInfo("arp", "-a")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                output = p.StandardOutput.ReadToEnd();

                p.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                }
            }

            return output;

        }
        /// <summary>
        /// Validates if the specified string is an IPv4 Address
        /// </summary>
        /// <param name="ipString">The string to compare</param>
        /// <returns>True if the address is valid.</returns>
        public static bool Validate(string ipString)
        {
            if (ipString == "0.0.0.0" || ipString == "127.0.0.1")
            {
                return false;
            }
            return new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$").IsMatch(ipString); //Check the octets
        }
    }
}