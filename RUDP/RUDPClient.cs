using System.Net;

namespace RUDP
{
    public class RUDPClient : BaseRUDPSocket
    {
        public RUDPClient(bool sigServer, int customPort = 0, string? bech32NSec = null) : base(sigServer, customPort, bech32NSec) { }


        public bool TryConnectWithSignalingServer(EndPoint ep, byte relativeIndex)
        {
            if (relativeIndex < 1 || relativeIndex > 3)
                return false;
            base.TryConnectWith(ep, relativeIndex);
            return true;
        }
        public bool TryConnectWithPeer(EndPoint ep)
        {
            base.TryConnectWith(ep, 0);
            return true;
        }
    }
}
