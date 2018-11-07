using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ShippingManager.USPS
{
    public class USPSManager : IShippingManager
    {
        private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string REMOVE_FROM_RATE_NAME = "&lt;sup&gt;&amp;reg;&lt;/sup&gt;";
        private readonly string _service;
        private readonly string _userId;

        public USPSManager()
        {
            _userId = "946CBCSC7886";
            _service = "ALL";
        }
        
        public List<ShippingRate> GetRates()
        {
            var sb = new StringBuilder();

            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("RateV4Request");
                writer.WriteAttributeString("USERID", _userId);

                writer.WriteElementString("Revision", "2");

                writer.WriteStartElement("Package");
                writer.WriteAttributeString("ID", "1");
                writer.WriteElementString("Service", _service);
                writer.WriteElementString("ZipOrigination", "90210");
                writer.WriteElementString("ZipDestination", "38017");
                writer.WriteElementString("Pounds", "15");
                writer.WriteElementString("Ounces", "0");

                writer.WriteElementString("Container", "RECTANGULAR");
                writer.WriteElementString("Size", "LARGE");
                writer.WriteElementString("Width", "13");
                writer.WriteElementString("Length", "12");
                writer.WriteElementString("Height", "14");
                writer.WriteElementString("Girth", "54");
                writer.WriteElementString("Machinable", "false");

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.Flush();
            }

            try
            {
                var url = string.Concat(PRODUCTION_URL, "?API=RateV4&XML=", sb.ToString());
                var webClient = new WebClient();
                var response = webClient.DownloadString(url);

                return ParseResult(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        private List<ShippingRate> ParseResult(string response)
        {
            List<ShippingRate> result = new List<ShippingRate>();
            var document = XElement.Parse(response, LoadOptions.None);

            var rates = from item in document.Descendants("Postage")
                        group item by (string)item.Element("MailService")
                        into g
                        select new { Name = g.Key, TotalCharges = g.Sum(x => Decimal.Parse((string)x.Element("Rate"), CultureInfo.InvariantCulture)) };

            foreach (var r in rates)
            {
                result.Add(new ShippingRate()
                {
                    Method = $"USPS {Regex.Replace(r.Name, "&lt.*&gt;", "")}",
                    Price = r.TotalCharges,
                });
            }

            return result;


            //check for errors
            //if (document.Descendants("Error").Any())
            //{
            //    var errors = from item in document.Descendants("Error")
            //                 select
            //                     new USPSError
            //                     {
            //                         Description = item.Element("Description").ToString(),
            //                         Source = item.Element("Source").ToString(),
            //                         HelpContext = item.Element("HelpContext").ToString(),
            //                         HelpFile = item.Element("HelpFile").ToString(),
            //                         Number = item.Element("Number").ToString()
            //                     };

            //    foreach (var err in errors)
            //    {
            //        AddError(err);
            //    }
            //}
        }

        public List<ShippingRate> GetRates(int numberOfExcecutions)
        {
            List<ShippingRate> result = new List<ShippingRate>();
            for (int i = 0; i < numberOfExcecutions; i++)
            {
                result.AddRange(GetRates());
            }

            return result;
        }
    }
}
