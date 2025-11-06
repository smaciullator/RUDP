namespace RUDP.Enums
{
    internal enum PacketType : byte
    {
        RTTA = 0, // Roung Trip Time A
        RTTB = 1, // Round Trip Time B
        ACKNOWLEDGEMENT = 2, // Acknowledgement
        DATA = 3, // Data
        STREAM = 4, // Stream
        MTU_DISCOVERY = 5, // MTU Discovery
        MTU_FOUND = 6, // MTU Found
        P2P_COORDINATION_REQUEST = 7, // Peer To Peer Coordination Request
        UNKNOWN_IDENTITY = 8, // Unknown Identity
        P2P_CONNECTION_COORDINATION = 9, // Peer To Peer Connection Coordination
        CONNECTION_POSSIBLE = 10, // Connection Possible
        SIGNALING_PROPAGATION = 11, // Signaling Propagation
        DISCONNECTION = 12, // Disconnect
        FILE_PRESENTATION = 13, // File Presentation
        FILE = 14, // File
    }
}
