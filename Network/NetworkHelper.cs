using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

namespace ETHAN.Network
{
    public static class NetworkHelper
    {
        /// <summary>
        /// Returns true if device has an active internet connection (WiFi or Mobile Data).
        /// </summary>
        public static bool IsConnected()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }


        /// <summary>
        /// Returns true if device is connected via WiFi.
        /// </summary>
        public static bool IsConnectedWifi()
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            return profiles.Contains(ConnectionProfile.WiFi);
        }

        /// <summary>
        /// Returns true if device is connected via Mobile Data.
        /// </summary>
        public static bool IsConnectedMobile()
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            return profiles.Contains(ConnectionProfile.Cellular);
        }

        /// <summary>
        /// Returns true if device has NO WiFi and NO Mobile Data.
        /// </summary>
        public static bool IsDisconnected()
        {
            return !IsConnected();
        }
    }
}
