using System;
using System.Collections.Generic;

namespace ShippingManager.FedEx
{
    public class FedExManager
    {
        public List<ShippingRate> GetRates()
        {
            List<ShippingRate> result = new List<ShippingRate>();

            RateRequest request = new RateRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "XXX"; 
            request.WebAuthenticationDetail.UserCredential.Password = "XXX"; 
           
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = "XXX"; 
            request.ClientDetail.MeterNumber = "XXX"; 
            
            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Rate Available Services Request using VC#***"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request);
            //

            RateService service = new RateService();
            RateReply reply = service.getRates(request);

            if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
            {
                foreach (var item in reply.RateReplyDetails)
                {
                    result.Add(new ShippingRate()
                    {
                        Method = item.ServiceType.ToString(),
                        Price = item.RatedShipmentDetails[0].ShipmentRateDetail.TotalNetCharge.Amount,
                    });
                }
            }

            return result;
        }

        private static void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.DropoffTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            SetOrigin(request);
            //
            SetDestination(request);
            //
            SetPackageLineItems(request);
            //
            request.RequestedShipment.PackageCount = "1";
            //set to true to request COD shipment
            bool isCodShipment = true;
            if (isCodShipment)
                SetCOD(request);
        }

        private static void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { "SHIPPER ADDRESS LINE 1" };
            request.RequestedShipment.Shipper.Address.City = "Austin";
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = "TX";
            request.RequestedShipment.Shipper.Address.PostalCode = "73301";
            request.RequestedShipment.Shipper.Address.CountryCode = "US";
        }

        private static void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { "RECIPIENT ADDRESS LINE 1" };
            request.RequestedShipment.Recipient.Address.City = "Collierville";
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = "TN";
            request.RequestedShipment.Recipient.Address.PostalCode = "38017";
            request.RequestedShipment.Recipient.Address.CountryCode = "US";
        }

        private static void SetPackageLineItems(RateRequest request)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";
            request.RequestedShipment.RequestedPackageLineItems[0].GroupPackageCount = "1";
            // package weight
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.LB;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.UnitsSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = 15.0M;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.ValueSpecified = true;
            // package dimensions
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions = new Dimensions();
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Length = "12";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Width = "13";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Height = "14";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Units = LinearUnits.IN;
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.UnitsSpecified = true;
        }

        private static void SetCOD(RateRequest request)
        {
            // To get all COD rates, set both COD details at both package and shipment level
            // Set COD at Package level for Ground Services
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested = new PackageSpecialServicesRequested();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes = new PackageSpecialServiceType[1];
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes[0] = PackageSpecialServiceType.COD;
            //
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail = new CodDetail();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CollectionType = CodCollectionType.GUARANTEED_FUNDS;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount = new Money();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Amount = 250;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.AmountSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Currency = "USD";
            // Set COD at Shipment level for Express Services
            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested(); // Special service requested
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = new ShipmentSpecialServiceType[1];
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes[0] = ShipmentSpecialServiceType.COD;
            //
            request.RequestedShipment.SpecialServicesRequested.CodDetail = new CodDetail();
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount = new Money();
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.Amount = 150;
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.AmountSpecified = true;
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.Currency = "USD";
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CollectionType = CodCollectionType.GUARANTEED_FUNDS;// ANY, CASH, GUARANTEED_FUNDS
        }
    }
}
