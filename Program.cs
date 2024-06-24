using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnumDrive
{
    public static class Program
    {
        static List<List<T>> Partition<T>(this List<T> values, int chunkSize)
        {
            return values.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        static async Task EnumAsync(string tenant, List<string> wordlist)
        {
            var part = wordlist.Partition(20);
            var c = part.Count();

            var parallelGroups = Enumerable.Range(0, c)
                                           .GroupBy(r => (r % 100));

            var users = new List<string>();

            var parallelTasks = parallelGroups.Select(groups =>
            {
                return Task.Run(async () =>
                {
                    foreach (var i in groups)
                    {
                        foreach (string u in part[i])
                        {
                            var url = $"https://{tenant}-my.sharepoint.com/personal/{u.Replace("@", "_").Replace(".", "_")}/_layouts/15/onedrive.aspx";
                            var client = new HttpClient();
                            var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                            if (result.StatusCode != HttpStatusCode.NotFound)
                            {
                                users.Add($"[>] User found: {u}");
                            }
                        }
                    }
                });
            });

            await Task.WhenAll(parallelTasks);
            Console.WriteLine(users.Any() ? string.Join("\n", users) : "[!] Users not found");
        }

        static List<string> GetTenant(string domain)
        {
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(@"https://autodiscover-s.outlook.com/autodiscover/autodiscover.svc");
            Req.Headers.Add(@"SOAPAction:http://schemas.microsoft.com/exchange/2010/Autodiscover/Autodiscover/GetFederationInformation");
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.UserAgent = "AutodiscoverClient";
            Req.Headers.Add("Accept-Encoding: identity");
            Req.Method = "POST";

            var data = $"<?xml version='1.0' encoding='utf-8'?><soap:Envelope xmlns:exm='http://schemas.microsoft.com/exchange/services/2006/messages' xmlns:ext='http://schemas.microsoft.com/exchange/services/2006/types' xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Header><a:Action soap:mustUnderstand='1'>http://schemas.microsoft.com/exchange/2010/Autodiscover/Autodiscover/GetFederationInformation</a:Action><a:To soap:mustUnderstand='1'>https://autodiscover-s.outlook.com/autodiscover/autodiscover.svc</a:To><a:ReplyTo><a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address></a:ReplyTo></soap:Header><soap:Body><GetFederationInformationRequestMessage xmlns='http://schemas.microsoft.com/exchange/2010/Autodiscover'><Request><Domain>{domain}</Domain></Request></GetFederationInformationRequestMessage></soap:Body></soap:Envelope>";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var tenants = new List<string>();
            var httpResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var r = streamReader.ReadToEnd();
                Regex pattern = new Regex(@"<Domain>(.*?)</Domain>", RegexOptions.Singleline, TimeSpan.FromSeconds(1));
                MatchCollection mLookup = pattern.Matches(r);
                foreach (Match match in mLookup)
                {
                    if (match.Groups[1].Value.Contains(".onmicrosoft.com") && !match.Groups[1].Value.Contains("mail.onmicrosoft.com"))
                        tenants.Add(match.Groups[1].Value.Replace(".onmicrosoft.com", ""));
                }
            }
            return tenants;
        }

        static bool ResolveHost(string domain)
        {
            try
            {
                IPAddress[] add = Dns.GetHostAddresses(domain);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 2 && args[0] is "recon")
                {
                    /*if (!ResolveHost(args[1]))
                    {
                        Console.WriteLine($"[!] Domain not found");
                        return;
                    }*/

                    List<string> tenants = GetTenant(args[1]);

                    if (!tenants.Any())
                    {
                        Console.WriteLine($"[!] Tenant not found");
                        return;
                    }

                    foreach (var t in tenants)
                        Console.WriteLine(ResolveHost($"{t}-my.sharepoint.com") ? $"[>] OneDrive found: {t}" : $"[!] OneDrive not found");
                }

                else if (args.Length == 3 && args[0] is "enum")
                {
                    List<string> file = File.ReadAllLines(args[2]).ToList();
                    if (!ResolveHost($"{args[1]}-my.sharepoint.com"))
                    {
                        Console.WriteLine($"[!] Tenant not found");
                        return;
                    }

                    await EnumAsync(args[1], file);
                }

                else
                {
                    Console.WriteLine("[!] Arguments error");
                    Console.WriteLine("[!] Use for get tenant: EnumDrive recon domain.com");
                    Console.WriteLine("[!] Use for enum users: EnumDrive enum tenant .\\users.txt");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[!] {e.Message}");
            }
        }
    }
}
