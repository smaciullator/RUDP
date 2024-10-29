using System.Net;
using System.Net.Sockets;

namespace RUDP.Utilities
{
    public static class NetworkUtilities
    {
        internal const int _classA_min = 1;
        internal const int _classA_max = 126;
        internal const int _classB_min = 128;
        internal const int _classB_max = 191;
        internal const int _classC_min = 192;
        internal const int _classC_max = 223;
        internal const int _classD_min = 224;
        internal const int _classD_max = 239;
        internal const int _classE_min = 240;
        internal const int _classE_max = 255;

        /// <summary>
        /// Restituisce l'ip locale della macchina all'interno della rete
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPAddress()
        {
            return GetLocalIPAddress(IpClass.C);
        }
        /// <summary>
        /// Restituisce l'ip locale della macchina all'interno della rete
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPAddress(IpClass ipClass)
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    int firstOctet = Convert.ToInt32(ip.ToString().Split('.')[0]);
                    if (ipClass == IpClass.A && firstOctet >= _classA_min && firstOctet <= _classA_max)
                        return ip;
                    else if (ipClass == IpClass.B && firstOctet >= _classB_min && firstOctet <= _classB_max)
                        return ip;
                    else if (ipClass == IpClass.C && firstOctet >= _classC_min && firstOctet <= _classC_max)
                        return ip;
                    else if (ipClass == IpClass.D && firstOctet >= _classD_min && firstOctet <= _classD_max)
                        return ip;
                    else if (ipClass == IpClass.E && firstOctet >= _classE_min && firstOctet <= _classE_max)
                        return ip;
                }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    public enum IpClass
    {
        A,
        B,
        C,
        D,
        E
    }
}
