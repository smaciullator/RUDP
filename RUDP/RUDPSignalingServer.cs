using System.Net;

namespace RUDP
{
    public class RUDPSignalingServer : BaseRUDPSocket
    {
        public RUDPSignalingServer(bool sigServer, int customPort = 0, string? bech32NSec = null) : base(sigServer, customPort, bech32NSec) { }


        public bool TryConnectWithSignalingServer(EndPoint ep)
        {
            base.TryConnectWith(ep, 0);
            return true;
        }
    }
}
