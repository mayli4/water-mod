using System.IO;
using WaterMod.Common.Networking;

namespace WaterMod.Utilities;

internal static class PacketExtensions {
    /* uncomment when C# 14 is supported in more places
    extension<T>(T inst) where T : unmanaged, IPacket {
        public void Write(BinaryWriter writer) {
            Packet<T>.Write(writer, in inst);
        }

        public static T Read(BinaryReader reader) {
            return Packet<T>.Read(reader);
        }

        public void Send(int toClient = -1, int ignoreClient = -1) {
            NetworkingHandler.SendPacket(in inst, toClient, ignoreClient);
        }
    }*/

    public static void Write<T>(this T inst, BinaryWriter writer) where T : unmanaged, IPacket {
        Packet<T>.Write(writer, in inst);
    }

    public static void Send<T>(this T inst, int toClient = -1, int ignoreClient = -1) where T : unmanaged, IPacket {
        NetworkingHandler.SendPacket(in inst, toClient, ignoreClient);
    }
}