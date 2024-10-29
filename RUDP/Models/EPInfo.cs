using RUDP.Extensions;
using RUDP.Keys;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;

namespace RUDP.Models
{
    public class EPInfo : IDisposable
    {
        public EndPoint EndPoint { get; private set; }
        public string IPV4Address => EndPoint is null ? "" : EndPoint.ToIPV4String();
        public bool IsConnectionPossible { get; private set; } = false;
        public bool IsTrusted { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public bool IsSigServer { get; private set; } = false;
        public bool? AmIConnecting { get; private set; } = null;
        public bool? AmIDisconnecting { get; private set; } = null;
        /// <summary>
        /// Become true if this endpoint is not properly sending RTTBs packets in a dynamic range of time (defined by this.UnackDropTime)
        /// </summary>
        public bool NotResponding => _rttaBuffer is not null && _rttaBuffer.Count > 0 && _rttaBuffer.Any(x => DateTime.Now.Ticks - x.Value > UnackDropTime);


        /// <summary>
        /// Define the maximum size of a single UDP packet
        /// </summary>
        public ushort MTUSize { get; private set; } = 1500;
        /// <summary>
        /// Used by Signaling Server to determine the relative index of a peer endpoint, relative to it's public 3 endpoints system
        /// </summary>
        internal byte RelativeIndex { get; private set; } = 0;
        /// <summary>
        /// Define the current unique numeration used for all packets (excepts STR)
        /// </summary>
        internal uint SendNumeration { get; private set; } = 0;
        /// <summary>
        /// Represent the cryptographycally secure random Int32 sent by me to this ep
        /// </summary>
        internal int? SentSecret { get; private set; } = null;
        /// <summary>
        /// Represent the cryptographycally secure random Int32 i received from this ep
        /// </summary>
        internal int? ReceivedSecret { get; private set; } = null;


        /// <summary>
        /// Contains the identity associated with this endpoint
        /// </summary>
        internal NPub? NPub { get; private set; } = null;
        /// <summary>
        /// The Bech32 representation of the endpoint's public identity
        /// </summary>
        public string? NPubBech32 => NPub is null ? null : NPub.Bech32;
        /// <summary>
        /// Contains informations about the quantity of packets and bytes sent and received per second with this peer
        /// </summary>
        public Rates Rates { get; private set; } = new();
        public double BytesUploadPerSecond => Rates.SentBytesPerSecond;
        public int PacketsUploadPerSecond => Rates.SentPacketsPerSecond;
        public double BytesDownloadPerSecond => Rates.ReceivedBytesPerSecond;
        public int PacketsDownloadPerSecond => Rates.ReceivedPacketsPerSecond;
        /// <summary>
        /// Contains a list of all sent but not yet unacknowledged RTTAs packets
        /// </summary>
        internal ConcurrentDictionary<uint, long> _rttaBuffer { get; private set; } = new();
        /// <summary>
        /// Contains the list of all packets sent but not yet acknowledged
        /// </summary>
        internal ConcurrentDictionary<string, UnackData> _unack { get; private set; } = new();
        /// <summary>
        /// Contains a list of all the data chunks received by this endpoint, but not yet finished, organized under multiple levels:
        ///     - UniqueIdentifier
        ///     - IV Bytes to utf8 string
        /// </summary>
        internal ConcurrentDictionary<uint, ConcurrentDictionary<string, ChunksInfo>> _chunks { get; set; } = new();


        private CongestionWindow CW { get; set; } = new();
        public double MaxUploadSpeed { get; internal set; } = 0;
        public double SafeBandWidth => CW.SafeBandWidth;
        /// <summary>
        /// Median latency for the last packets exchanged with this endpoint
        /// </summary>
        public double MedianLatency => CW.RTTMedian;
        /// <summary>
        /// Represent the time that have to pass before sending againg an unacknowledged packet.
        /// Expressed in Ticks
        /// </summary>
        internal double UnackRetryTime => Math.Max(500, CW.RTTMedian * 2) * 10000;
        /// <summary>
        /// Represent the time that have to pass before dropping an unacknowledged packet.
        /// Expressed in Ticks
        /// </summary>
        internal double UnackDropTime => Math.Max(50000, CW.RTTMedian * 60) * 10000;
        /// <summary>
        /// Represent the time that have to pass from the moment an endpoint has started not responding to automatically disconnect
        /// Expressed in Ticks
        /// </summary>
        internal double NotRespondingAutoDisconnectionTime => Math.Max(30000, CW.RTTMedian * 150) * 10000;


        public EPInfo() { }
        /// <summary>
        /// Set an initial MTUSize of 1500 bytes
        /// </summary>
        /// <param name="endPoint"></param>
        public EPInfo(EndPoint endPoint)
        {
            EndPoint = endPoint;
        }
        /// <summary>
        /// Set the initial MTUSize as the value passed
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="mtuSize"></param>
        internal EPInfo(EndPoint endPoint, ushort mtuSize)
        {
            EndPoint = endPoint;
            SetMTUSize(mtuSize);
        }
        public EPInfo(EPInfo info)
        {
            EndPoint = info.EndPoint;
            IsTrusted = info.IsTrusted;
            IsConnected = info.IsConnected;
            IsSigServer = info.IsSigServer;
            AmIDisconnecting = info.AmIDisconnecting;
            MTUSize = info.MTUSize;
            NPub = info.NPub;
            Rates = info.Rates;
        }


        internal double CalculateCongestionWindow(double newRTTmilliseconds)
        {
            long now = DateTime.Now.Ticks;

            CW.SetStatistics(newRTTmilliseconds);

            // If we are not yet initialized we optimistically double the Congestion Window
            if (!CW.Initialized)
                CW.SafeBandWidth = CW.MinSafeBandWidth * 2;
            else
            {
                // If this RTT is more than double of median RTT (before adding the new value itself) we should drastically reduce the Congestion Window
                if (newRTTmilliseconds > (CW.RTTMedian * 10000000) * 2)
                {
                    CW.SafeBandWidth /= 2; // Multiplicative Decrease
                    CW.MaxRecordedSafeBandWidth -= CW.SafeBandWidth / 2; // We also decrease by a quarter the MaxRecorderSafeBandWidth
                }
                // If the are some RTTA packets unacknowledged by more than double of median RTT we should proportionally reduce the Congestion Window
                else if (
                    _rttaBuffer.Any(x => now - x.Value > (CW.RTTMedian * 10000000) * 2)
                    || _unack.Count(x => now - x.Value.Timestamp > (CW.RTTMedian * 10000000) * 2) > 3
                )
                {
                    // If under the slow start threshold
                    if (CW.SafeBandWidth <= CW.MaxRecordedSafeBandWidth)
                    {
                        CW.SafeBandWidth -= CW.MinSafeBandWidth; // Decrease
                        CW.MaxRecordedSafeBandWidth -= CW.MinSafeBandWidth;
                    }
                    else
                    {
                        CW.SafeBandWidth /= 2; // Multiplicative Decrease
                        CW.MaxRecordedSafeBandWidth -= CW.SafeBandWidth / 2; // We also decrease by a quarter the MaxRecorderSafeBandWidth
                    }
                }
                else
                {
                    // If under the slow start threshold
                    if (CW.SafeBandWidth <= CW.MaxRecordedSafeBandWidth)
                        CW.SafeBandWidth *= 2; // Slow Start
                    else
                        CW.SafeBandWidth += CW.MinSafeBandWidth; // Additive Increase
                }
            }

            // If a max upload speed limit has been set we always use it as a ceiling
            if (MaxUploadSpeed > 0)
            {
                CW.SafeBandWidth = Math.Max(MaxUploadSpeed, CW.SafeBandWidth);
                CW.MaxRecordedSafeBandWidth = Math.Max(MaxUploadSpeed, CW.MaxRecordedSafeBandWidth);
            }

            return CW.SafeBandWidth;
        }


        internal EPInfo SetIsConnectionPossible(bool isConnectionPossible)
        {
            IsConnectionPossible = isConnectionPossible;
            return this;
        }
        internal EPInfo SetConnected(bool isConnected)
        {
            IsConnected = isConnected;
            return this;
        }
        internal EPInfo SetTrusted(bool isTrusted)
        {
            IsTrusted = isTrusted;
            return this;
        }
        internal EPInfo SetIsSigServer(bool isSigServer)
        {
            IsSigServer = isSigServer;
            return this;
        }
        internal EPInfo SetAmIDisconnecting(bool? amIDisconnecting)
        {
            AmIDisconnecting = amIDisconnecting;
            return this;
        }
        internal EPInfo SetAmIConnecting(bool? amIConnecting)
        {
            AmIConnecting = amIConnecting;
            return this;
        }
        internal EPInfo SetRates(Rates rates)
        {
            Rates = new()
            {
                ReceivedBytesPerSecond = rates.ReceivedBytesPerSecond,
                ReceivedPacketsPerSecond = rates.ReceivedPacketsPerSecond,
                SentBytesPerSecond = rates.SentBytesPerSecond,
                SentPacketsPerSecond = rates.SentPacketsPerSecond
            };
            return this;
        }
        internal EPInfo SetRelativeIndex(byte relativeIndex)
        {
            RelativeIndex = relativeIndex;
            return this;
        }


        internal EPInfo AddUnackPacket(byte[] packet)
        {
            UnackData unack = new(packet);
            if (unack.Timestamp.HasValue)
                _unack.TryAdd(unack.UID, unack);
            return this;
        }
        internal EPInfo AddRTTA(uint uniqueIdentifier)
        {
            _rttaBuffer.TryAdd(uniqueIdentifier, DateTime.Now.Ticks);
            return this;
        }
        internal EPInfo RemoveRTTA(uint uniqueIdentifier)
        {
            _rttaBuffer.TryRemove(uniqueIdentifier, out _);
            return this;
        }
        /// <summary>
        /// Clear all the unacknowledged RTTAs sent before the specified moment, expressed in Ticks
        /// </summary>
        /// <param name="now"></param>
        internal void ClearEarlierRTTAs(long now)
        {
            if (AmIDisconnecting.HasValue)
                return;
            foreach (KeyValuePair<uint, long> rtta in _rttaBuffer)
                if (rtta.Value <= now)
                    _rttaBuffer.TryRemove(rtta.Key, out _);
        }


        internal EPInfo SetMTUSize(ushort mtuSizeValue)
        {
            MTUSize = Convert.ToUInt16(Math.Max(Convert.ToUInt16(65), mtuSizeValue));
            CW.MinSafeBandWidth = Convert.ToDouble(MTUSize);
            return this;
        }
        /// <summary>
        /// Takes the amount of bytes that throwed an MTU SIZE EXCEED Exception and try to reduce the current MTU Size
        /// by 1 bytes, also taking into account the current value with the following logic:
        ///     - take the minimum value between "mtuSizeExceededValue - 1" and the current MTUSize - 1
        ///     - then take the maximum value between 14 and the result of the previous check
        /// </summary>
        /// <param name="mtuSizeExceededValue"></param>
        /// <returns></returns>
        internal EPInfo ReduceMTUSize(ushort mtuSizeExceededValue)
        {
            MTUSize = Convert.ToUInt16(Math.Max(Convert.ToUInt16(65), Math.Min(mtuSizeExceededValue - 1, MTUSize - 1)));
            return this;
        }


        /// <summary>
        /// Set the current secret randomly generated and sent by me to this endpoint
        /// </summary>
        /// <returns></returns>
        internal EPInfo SetSentSecret()
        {
            int secret = RandomNumberGenerator.GetInt32(int.MaxValue);
            while (secret == 0)
                secret = RandomNumberGenerator.GetInt32(int.MaxValue);
            SentSecret = secret;
            return this;
        }
        /// <summary>
        /// Set the current secret randomly generated and sent by this endpoint to me
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        internal EPInfo SetReceivedSecret(int secret)
        {
            ReceivedSecret = secret;
            return this;
        }


        /// <summary>
        /// Sets the NPub identity
        /// </summary>
        /// <param name="npub"></param>
        internal EPInfo SetNPub(NPub? npub)
        {
            NPub = npub;
            return this;
        }


        /// <summary>
        /// Return the next unique numeration for DATA packets.
        /// NOTE: if the numeration reach the end (ulong.MaxValue) it reset to 0
        /// </summary>
        /// <returns></returns>
        internal uint GetNextSendNumeration()
        {
            if (SendNumeration == uint.MaxValue)
                SendNumeration = uint.MinValue;
            return ++SendNumeration;
        }


        public void Dispose()
        {
            CW.Dispose();
            _unack.Clear();
            _rttaBuffer.Clear();
            foreach (KeyValuePair<uint, ConcurrentDictionary<string, ChunksInfo>> chunk in _chunks)
            {
                foreach (KeyValuePair<string, ChunksInfo> subChunk in chunk.Value)
                    subChunk.Value.Dispose();
                chunk.Value.Clear();
            }
            _chunks.Clear();
            ReceivedSecret = null;
            SentSecret = null;
        }
    }
}
