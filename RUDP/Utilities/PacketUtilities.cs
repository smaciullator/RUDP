namespace RUDP.Utilities
{
    internal static class PacketUtilities
    {
        internal static byte[] CreatePacket(Header header, byte[]? data = null)
        {
            byte[] head = header.Serialize();
            if (data is null)
                return head;
            byte[] pkt = new byte[head.Length + data.Length];
            Array.Copy(head, 0, pkt, 0, head.Length);
            Array.Copy(data, 0, pkt, head.Length, data.Length);
            head = new byte[0];
            return pkt;
        }
    }
}
