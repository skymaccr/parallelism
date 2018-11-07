using System.Collections.Generic;

namespace ShippingManager
{
    public interface IShippingManager
    {
        List<ShippingRate> GetRates();
        List<ShippingRate> GetRates(int numberOfExcecutions);
    }
}
