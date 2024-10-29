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
        HANDSHAKE = 7, // Handshake
        CONNECTION_CONFIRM = 8, // Connection Confirm
        P2P_COORDINATION_REQUEST = 9, // Peer To Peer Coordination Request
        UNKNOWN_IDENTITY = 10, // Unknown Identity
        P2P_CONNECTION_COORDINATION = 11, // Peer To Peer Connection Coordination
        CONNECTION_POSSIBLE = 12, // Connection Possible
        SIGNALING_PROPAGATION = 13, // Signaling Propagation
        DISCONNECTION = 14, // Disconnect
        FILE_PRESENTATION = 15, // File Presentation
        FILE = 16, // File
    }
}
