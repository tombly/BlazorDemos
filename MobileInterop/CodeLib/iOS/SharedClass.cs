namespace CodeLib
{
    public class SharedClass
    {
        /// <summary>
        /// A simple method.
        /// </summary>
        public static string GetPlatformName()
        {
            return "IOS";
        }

        /// <summary>
        /// A method that invokes device APIs.
        /// </summary>
        public static async Task<string?> GetPostalCode(string address)
        {
            using (var geocoder = new CoreLocation.CLGeocoder())
            {
                var addr = await geocoder.GeocodeAddressAsync(address);
                return addr?.First().PostalCode;
            }
        }

        /// <summary>
        /// A method that uses MAUI Essentials.
        /// </summary>
        public static string GetConnectivity()
        {
            return $"{Microsoft.Maui.Networking.Connectivity.NetworkAccess}";
        }
    }
}
