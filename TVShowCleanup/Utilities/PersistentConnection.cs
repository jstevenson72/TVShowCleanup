using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace TVShowCleanup.Utilities
{
    public class PersistentConnection
    {
        string _networkName;

        public PersistentConnection(string networkName,
            NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            CleanupLog.WriteLine($"Connection Result: {result}");

            if (result != 0 && result != 1219)
            {
                throw new Win32Exception(result, "Error connecting to remote share");
            }
        }
       
        public void CloseConnection()
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }
}
