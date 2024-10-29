namespace RUDP.Enums
{
    internal enum NATPortAlgorithm : byte
    {
        /// <summary>
        /// Indipendent NAT, will give same ip:port for different endpoints
        /// </summary>
        Same,
        /// <summary>
        /// Symmetric NAT that for each new ip:port assign a port number equal to the previous +1
        /// </summary>
        Incremental,
        /// <summary>
        /// Symmetric NAT that for each new ip:port assign a port number equal to the previous -1
        /// </summary>
        Decremental,
        /// <summary>
        /// Symmetric NAT that for each new ip:port assign a port number at a given interval (higher or lower) in respect to the previous port
        /// </summary>
        Skip,
        /// <summary>
        /// Symmetric NAT that for each new ip:port assign a new random port number
        /// </summary>
        Random,
        /// <summary>
        /// NAT Algorithm unrecognized
        /// </summary>
        Uncatchable
    }
}
