using System.Net;

namespace RUDP.Extensions
{
    public static class EndPointExtensions
    {
        /// <summary>
        /// Confronta Address e Port dell'endpoint con l'endpoint indicato e restituisce un booleano ad indicare se sono identici
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="ep2"></param>
        /// <returns></returns>
        public static bool EqualTo(this EndPoint ep, EndPoint? ep2)
        {
            if (ep is null || ep2 is null)
                return false;
            IPEndPoint ip1 = (IPEndPoint)ep;
            IPEndPoint ip2 = (IPEndPoint)ep2;
            return ip1.Address.ToString() == ip2.Address.ToString() && ip1.Port == ip2.Port;
        }

        /// <summary>
        /// Restituisce una stringa IPV4 a partire dall'endpoint indicato
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        public static string? ToIPV4String(this EndPoint? ep)
        {
            if (ep is null)
                return null;
            IPEndPoint ipEndPoint = (IPEndPoint)ep;
            return ipEndPoint.Address + ":" + ipEndPoint.Port;
        }
    }
}
