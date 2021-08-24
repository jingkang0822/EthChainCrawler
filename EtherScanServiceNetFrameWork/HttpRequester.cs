using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace EtherScanServiceNetFrameWork
{
    public class HttpRequester
    {
        private static object _locker = new object();

        private void ApiThreadWaiting()
        {
            lock (_locker)
            {
                // To prevent web-site block crawler
                Thread.Sleep((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
        }

        public string GetHtmlResponse(string url)
        {
            ApiThreadWaiting();

            var request = HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetHtmlResponse] error: {ex.Message}");
                return null;
            }
        }
    }
}
