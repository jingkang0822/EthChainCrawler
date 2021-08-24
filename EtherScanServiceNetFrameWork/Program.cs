using EtherscanApi.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    class Program
    {
        static void Main(string[] args)
        {
            EtherScanService.Start();

            Console.WriteLine("Service is up.");
            Console.ReadLine();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("exit");
        }
    }
}
