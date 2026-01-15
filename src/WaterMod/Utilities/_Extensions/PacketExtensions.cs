using System.IO;
using WaterMod.Common.Networking;

namespace WaterMod.Utilities;

internal static class PacketExtensions
{
    extension<T>(T inst) where T : unmanaged, IPacket
    {
        public void Write(BinaryWriter writer)
        {
            Packet<T>.Write(writer, in inst);
        }

        public static T Read(BinaryReader reader)
        {
            return Packet<T>.Read(reader);
        }

        public void Send(int toClient = -1, int ignoreClient = -1)
        {
            NetworkingHandler.SendPacket(in inst, toClient, ignoreClient);
        }
    }
}