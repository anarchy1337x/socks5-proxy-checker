using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp24
{
    class Program
    {
        public static int valid = 0;
        public static int invalid = 0;
        public static void WriteLine(string text)
        {
            lock(Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("[INFO] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text);
                Console.WriteLine();
            }
        }
        public static List<string> Proxies = new List<string>();
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(500, 500);
            WriteLine($"Loading proxies...");
            Parallel.ForEach(File.ReadAllLines("proxies.txt").AsEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = 500 }, proxy =>
            {
                Proxies.Add(proxy);
            });
            WriteLine($"Loaded '{Proxies.Count}' proxies.");
            WriteLine($"Starting checker... ");
            Parallel.ForEach(File.ReadAllLines("proxies.txt").AsEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = 500 }, proxy =>
            {
                CheckProxy(proxy);
            });
            Console.ReadLine();
        }
        public static void CheckProxy(string proxy)
        {
            string[] split = proxy.Split(':');
            HttpRequest request = new HttpRequest
            {
                UserAgent = Http.ChromeUserAgent(),
                IgnoreProtocolErrors = true,
                ConnectTimeout = 5000,
                KeepAliveTimeout = 1000,
                ReadWriteTimeout = 1000
            };
            request.Proxy = Socks5ProxyClient.Parse(ProxyType.Socks5, proxy);
            try
            {
                string output = request.Get("http://api.proxychecker.co/").ToString();
                lock (Console.Out)
                {
                    WriteLine($"{split[0]}:{split[1]}");
                    Console.Title = $"ProxyChecker | Valid: {valid} | Invalid: {invalid}";
                    valid++;
                }
            }
            catch
            {
                Console.Title = $"ProxyChecker | Valid: {valid} | Invalid: {invalid}";
                invalid++;
                request?.Dispose();
            }
            finally
            {
                request?.Dispose();
            }
        }
    }
}
