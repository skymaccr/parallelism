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

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            int secuentialWins = 0, parallelwins = 0;
            decimal secuentialTotal = 0, parallelTotal = 0;

            for (int i = 0; i < 100; i++)
            {
                Task<long> sequential = Task.Factory.StartNew(() =>
                {
                    return CalculateSequential();
                });

                Task<long> parallel = Task.Factory.StartNew(() =>
                {
                    return CalculateParallel();
                });

                Task.WaitAll(new Task[] { sequential, parallel });

                secuentialTotal += sequential.Result / 1000;
                parallelTotal += parallel.Result / 1000;

                if (sequential.Result > parallel.Result)
                    parallelwins++;
                else
                    secuentialWins++;
            }

            if (parallelwins < secuentialWins)
                Console.WriteLine($"Secuential wins {secuentialWins} to {parallelwins}");
            else
                Console.WriteLine($"Parallel wins {parallelwins} to {secuentialWins}");

            Console.WriteLine($"Sequential Total{secuentialTotal} seconds - Pararallel Total {parallelTotal} seconds");

            Console.ReadLine();
        }

        private static long CalculateParallel()
        {
            var stopWatch = Stopwatch.StartNew();
            stopWatch.Start();
            ConcurrentBag<ShippingRate> rates = new ConcurrentBag<ShippingRate>();

            var providers = new List<IShippingManager>
            {
                //Get FedEx
                new FedExManager(),
                //Get USPS
                new USPSManager()
            };

            Task<DolarExchangeRate> exchangeRates = Task.Factory.StartNew(() =>
            {
                //Get Bank Exchange Rates
                IBankManager bank = new BNCRManager();
                return bank.GetExchangeRate();
            });

            Parallel.ForEach(providers, provider =>
            {
                foreach (var rate in provider.GetRates())
                {
                    rates.Add(rate);
                }
            });

            Task.WaitAll(new Task[] { exchangeRates, /*fedexTask, uspsTask*/ });

            var ratesInColones = rates.Select(c => { c.Price = c.Price * exchangeRates.Result.PurchasePrice; return c; }).ToList();

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        private static long CalculateSequential()
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
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
