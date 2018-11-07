using BankManager;
using ShippingManager;
using ShippingManager.FedEx;
using ShippingManager.USPS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelExcecution
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            //List<ShippingRate> rates = new List<ShippingRate>();
            ConcurrentBag<ShippingRate> rates = new ConcurrentBag<ShippingRate>();

            var providers = new List<IShippingManager>
            {
                //Get FedEx
                new FedExManager(),
                //Get USPS
                new USPSManager()
            };

            //IShippingManager fedEx = new FedExManager();
            //IShippingManager usps = new USPSManager();

            Task<DolarExchangeRate> exchangeRates = Task.Factory.StartNew(() =>
            {
                //Get Bank Exchange Rates
                IBankManager bank = new BNCRManager();
                return bank.GetExchangeRate();
            });

            //Task<List<ShippingRate>> fedexTask = Task.Factory.StartNew(() =>
            //{
            //    return fedEx.GetRates();
            //});

            //Task<List<ShippingRate>> uspsTask = Task.Factory.StartNew(() =>
            //{
            //    return usps.GetRates();
            //});

            Parallel.ForEach(providers, provider =>
            {
                foreach (var rate in provider.GetRates())
                {
                    rates.Add(rate);
                }
            });

            Task.WaitAll(new Task[] { exchangeRates, /*fedexTask, uspsTask*/ });

            //rates.AddRange(fedexTask.Result);
            //rates.AddRange(uspsTask.Result);

            var ratesInColones = rates.Select(c => { c.Price = c.Price * exchangeRates.Result.PurchasePrice; return c; }).ToList();

            stopWatch.Stop();
            Console.WriteLine($"Parallel Time {stopWatch.ElapsedMilliseconds}");

            Console.WriteLine("Shipping Method          Price (Colones)");
            foreach (var rate in ratesInColones)
            {
                Console.WriteLine($"{rate.Method}           {rate.Price}");
            }

            //Console.ReadKey();
        }
    }
}
