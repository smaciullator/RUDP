using RUDP.Extensions;
using System.Net;

namespace RUDP.Models
{
    public class PeerEPs
    {
        public EndPoint? EP1 { get; set; }
        public EndPoint? EP2 { get; set; }
        public EndPoint? EP3 { get; set; }
        public EndPoint? ConnectedEP
        {
            get
            {
                if (!Connected)
                    return null;
                else if (RelativeIndex == 1)
                    return EP1;
                else if (RelativeIndex == 2)
                    return EP2;
                else if (RelativeIndex == 3)
                    return EP3;
                else
                {
                    if (EP1 is not null)
                        return EP1;
                    if (EP2 is not null)
                        return EP2;
                    if (EP3 is not null)
                        return EP3;
                }
                return null;
            }
        }
        public byte RelativeIndex { get; set; } = 0;
        public bool Connected { get; set; } = false;


        public PeerEPs() { }
        public PeerEPs(EndPoint? ep, byte relativeIndex, bool connected)
        {
            SetRelativeIndexEndPoint(ep, relativeIndex, connected);
        }
        public PeerEPs(EndPoint? eP1, EndPoint? eP2, EndPoint? eP3, byte relativeIndex, bool connected)
        {
            EP1 = eP1;
            EP2 = eP2;
            EP3 = eP3;
            RelativeIndex = relativeIndex;
            Connected = connected;
        }


        public PeerEPs SetEndPoint(EndPoint? ep, byte targetRelativeIndex, bool connected)
        {
            Connected = connected;
            if (targetRelativeIndex == 1)
                EP1 = ep;
            else if (targetRelativeIndex == 2)
                EP2 = ep;
            else if (targetRelativeIndex == 3)
                EP3 = ep;
            return this;
        }
        public PeerEPs SetRelativeIndexEndPoint(EndPoint? ep, byte relativeIndex, bool connected)
        {
            RelativeIndex = relativeIndex;
            SetEndPoint(ep, relativeIndex, connected);
            return this;
        }


        public bool IsValid()
        {
            return EP1 is not null;
        }
        public bool EndPointIsKnown(EndPoint ep)
        {
            return EP1.EqualTo(ep) || EP2.EqualTo(ep) || EP3.EqualTo(ep);
        }
        /// <summary>
        /// Used to try to retrieve the EndPoint this peer has with me, using the RelativeIndex he sent on early phases.
        /// This method try to return the correct EndPoint or null in case we don't have a connection with this peer.
        /// </summary>
        /// <returns></returns>
        public EndPoint? GetKnownEndpoint()
        {
            // If we only received this peer info throught a SIGNALING_PROPAGATION
            if (!Connected || RelativeIndex == 0)
                return EP1;
            else if (RelativeIndex == 1)
                return EP1;
            else if (RelativeIndex == 2)
                return EP2;
            else if (RelativeIndex == 3)
                return EP3;
            return null;
        }
    }
}
