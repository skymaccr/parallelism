using RestSharp;

namespace BankManager
{
    public class BNCRManager : IBankManager
    {        
        public DolarExchangeRate GetExchangeRate()
        {
            var client = new RestClient("https://bncrappsmobappprod.azurewebsites.net");

            IRestRequest request = new RestRequest("api/ConsultaTipoCambios", Method.GET);
            request.AddParameter("pCanal", "IBP");

            IRestResponse<BNCRExchangeRates> response = client.Execute<BNCRExchangeRates>(request);

            var exchangeRate = new DolarExchangeRate
            {
                PurchasePrice = response.Data.compraDolares,
                SellingPrice = response.Data.ventaDolares
            };

            return exchangeRate;
        }
    }
}
