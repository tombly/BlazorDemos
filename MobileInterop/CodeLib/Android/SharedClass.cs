namespace CodeLib
{
    public class SharedClass
    {
        /// <summary>
        /// A simple method.
        /// </summary>
        public static string GetPlatformName()
        {
            return "ANDROID";
        }

        /// <summary>
        /// A method that invokes device APIs.
        /// </summary>
        public static async Task<string?> GetPostalCode(string address)
        {
            using (var geocoder = new Android.Locations.Geocoder(Android.App.Application.Context))
            {
                var addr = await geocoder.GetFromLocationNameAsync(address, 1);
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
