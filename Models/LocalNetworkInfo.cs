using System.Net;
using System.Net.Sockets;

namespace TankBattleOnline
{
    public static class LocalNetworkInfo
    {
        public static string GetLocalIPv4()
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    {
                        return address.ToString();
                    }
                }
            }
            catch
            {
            }

            return "127.0.0.1";
        }
    }
}
