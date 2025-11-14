namespace RUDP.Enums
{
    internal enum PacketType : byte
    {
        RTTA = 0, // Roung Trip Time A
        RTTB = 1, // Round Trip Time B
        ACKNOWLEDGEMENT = 2, // Acknowledgement
        CHUNKS_PRESENTATION = 3, // Round Trip Time B
        DATA = 4, // Data
        STREAM = 5, // Stream
        MTU_DISCOVERY = 6, // MTU Discovery
        MTU_FOUND = 7, // MTU Found
        P2P_COORDINATION_REQUEST = 8, // Peer To Peer Coordination Request
        UNKNOWN_IDENTITY = 9, // Unknown Identity
        P2P_CONNECTION_COORDINATION = 10, // Peer To Peer Connection Coordination
        CONNECTION_POSSIBLE = 11, // Connection Possible
        SIGNALING_PROPAGATION = 12, // Signaling Propagation
        DISCONNECTION = 13, // Disconnect
        FILE_PRESENTATION = 14, // File Presentation
        FILE_CONTENT = 15, // File
    }
}
