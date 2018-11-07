using BankManager;
using ShippingManager;
using ShippingManager.FedEx;
using ShippingManager.USPS;
using System;
using System.Collections.Generic;

namespace SequentialExecution
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            List<ShippingRate> rates = new List<ShippingRate>();

            //Get FedEx
            FedExManager fedEx = new FedExManager();
            rates.AddRange(fedEx.GetRates());

            //Get USPS
            USPSProvider usps = new USPSProvider();
            rates.AddRange(usps.GetRates());

            //Get Bank Exchange Rates
            BNCRManager bncr = new BNCRManager();
            var exchangeRates = bncr.GetExchangeRate();


            foreach (var item in rates)
            {
                item.Price = item.Price * exchangeRates.PurchasePrice;
            }

            Console.WriteLine("Shipping Method          Price (Colones)");
            foreach (var item in rates)
            {
                Console.WriteLine($"{item.Method}           {item.Price}");
            }

            DateTime endTime = DateTime.Now;

            Console.WriteLine($"Sequential Time {endTime.Subtract(startTime).Milliseconds}");

            Console.ReadKey(); 
        }
    }
}
