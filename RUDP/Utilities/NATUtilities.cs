using RUDP.Enums;
using System.Net;

namespace RUDP.Utilities
{
    internal static class NATUtilities
    {
        /// <summary>
        /// Given 3 optional endpoints, this method will return the (most likely true) NAT algorithm of the peer based on pure assumptions.
        /// In the two out parameters we have:
        ///     - nextPort: the most plausible next port to try punch
        ///     - skipRandomMaxInterval: greater than 0 only for NATPortAlgorithm.Skip and NATPortAlgorithm.Random, contains the greater interval
        ///       bewteen the ports
        /// </summary>
        /// <param name="ep1"></param>
        /// <param name="ep2"></param>
        /// <param name="ep3"></param>
        /// <param name="nextPort"></param>
        /// <param name="skipRandomMaxInterval"></param>
        /// <returns></returns>
        internal static NATPortAlgorithm GuessUserNATPortAlgorithm(EndPoint? ep1, EndPoint? ep2, EndPoint? ep3, out int nextPort, out int skipRandomMaxInterval)
        {
            nextPort = 0;
            skipRandomMaxInterval = 1000;
            if (ep1 is null && ep2 is null && ep3 is null)
                return NATPortAlgorithm.Uncatchable;

            // Single notification, we go straight to the only known port number
            if (ep1 is not null && ep2 is null && ep3 is null)
            {
                nextPort = ((IPEndPoint)ep1).Port;
                return NATPortAlgorithm.Same;
            }
            // Double notification, more precise but still fail to distinguish between Skip and Random NAT
            else if (ep1 is not null && ep2 is not null && ep3 is null)
            {
                IPEndPoint ip1 = (IPEndPoint)ep1;
                IPEndPoint ip2 = (IPEndPoint)ep2;
                if (ip1.Address.ToString() != ip2.Address.ToString())
                    return NATPortAlgorithm.Uncatchable;
                else if (ip1.Port == ip2.Port)
                {
                    nextPort = ip1.Port;
                    return NATPortAlgorithm.Same;
                }
                else if (ip1.Port + 1 == ip2.Port)
                {
                    nextPort = ip2.Port + 1;
                    return NATPortAlgorithm.Incremental;
                }
                else if (ip1.Port == ip2.Port - 1)
                {
                    nextPort = ip2.Port - 1;
                    return NATPortAlgorithm.Decremental;
                }
                else
                {
                    // Here we lack the third endpoint to check if it's a Random NAT
                    int interval = Math.Abs(ip2.Port - ip1.Port);
                    if (interval > 1)
                        nextPort = ip2.Port + interval;
                    else if (interval < -1)
                        nextPort = ip2.Port - interval;
                    skipRandomMaxInterval = Math.Abs(interval);
                    return NATPortAlgorithm.Skip;
                }
            }
            // Triple notification, should be able to catch every type of NAT
            else if (ep1 is not null && ep2 is not null && ep3 is not null)
            {
                IPEndPoint ip1 = (IPEndPoint)ep1;
                IPEndPoint ip2 = (IPEndPoint)ep2;
                IPEndPoint ip3 = (IPEndPoint)ep3;
                if (ip1.Address.ToString() != ip2.Address.ToString() || ip2.Address.ToString() != ip3.Address.ToString() || ip1.Address.ToString() != ip3.Address.ToString())
                    return NATPortAlgorithm.Uncatchable;
                else if (ip1.Port == ip2.Port && ip2.Port == ip3.Port)
                {
                    nextPort = ip1.Port;
                    return NATPortAlgorithm.Same;
                }
                else if (ip1.Port + 1 == ip2.Port && ip2.Port + 1 == ip3.Port)
                {
                    nextPort = ip2.Port + 1;
                    return NATPortAlgorithm.Incremental;
                }
                else if (ip1.Port == ip2.Port - 1 && ip2.Port == ip3.Port - 1)
                {
                    nextPort = ip2.Port - 1;
                    return NATPortAlgorithm.Decremental;
                }
                else
                {
                    int interval1 = Math.Abs(ip2.Port - ip1.Port),
                        interval2 = Math.Abs(ip3.Port - ip2.Port);

                    // Same interval
                    if (interval1 == interval2)
                    {
                        if (interval1 > 1)
                            nextPort = ip3.Port + interval1;
                        else if (interval1 < -1)
                            nextPort = ip3.Port - interval1;
                        return NATPortAlgorithm.Skip;
                    }

                    // Different intervals, we start from the first port
                    nextPort = ip1.Port;
                    skipRandomMaxInterval = Math.Max(Math.Abs(interval1), Math.Abs(interval2));
                    return NATPortAlgorithm.Random;
                }
            }
            return NATPortAlgorithm.Uncatchable;
        }
    }
}
