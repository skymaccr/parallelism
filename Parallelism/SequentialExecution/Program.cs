using BankManager;
using ShippingManager;
using ShippingManager.FedEx;
using ShippingManager.USPS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SequentialExecution
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            //Get Bank Exchange Rates
            BNCRManager bank = new BNCRManager();
            var exchangeRates = bank.GetExchangeRate();

            List<ShippingRate> rates = new List<ShippingRate>();

            FedExManager fedExManager = new FedExManager();
            rates.AddRange(fedExManager.GetRates());

            USPSManager uspsManager = new USPSManager();
            rates.AddRange(uspsManager.GetRates());

            var ratesInColones = rates.Select(c => { c.Price = c.Price * exchangeRates.PurchasePrice; return c; }).ToList();

            stopWatch.Stop();
            Console.WriteLine($"Sequential Time {stopWatch.ElapsedMilliseconds}");

            Console.WriteLine("Shipping Method          Price (Colones)");
            foreach (var rate in rates)
            {
                Console.WriteLine($"{rate.Method}           {rate.Price}");
            }

            //Console.ReadKey(); 
        }
    }
}
