namespace CodeLib
{
    public class SharedClass
    {
        /// <summary>
        /// A simple method.
        /// </summary>
        public static string GetPlatformName()
        {
            return "WEB (.NET)";
        }

        /// <summary>
        /// A method that invokes device APIs (which isn't possible in the
        /// .NET target).
        /// </summary>
        public static Task<string> GetPostalCode(string address)
        {
            // We don't have access to the device APIs from .NET.
            return Task.FromResult("Not Available");
        }

        /// <summary>
        /// A method that uses MAUI Essentials (which won't work from the
        /// .NET target).
        /// </summary>
        public static string GetConnectivity()
        {
            // If we call Microsoft.Maui.Networking.Connectivity.NetworkAccess
            // then we'll get an exception because there is no MAUI implementation
            // for plain .NET.
            throw new NotImplementedException();
        }
    }
}
