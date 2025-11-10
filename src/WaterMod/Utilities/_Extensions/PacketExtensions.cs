using System.IO;
using WaterMod.Common.Networking;

namespace WaterMod.Utilities;

internal static class PacketExtensions {
    extension<T>(ref T inst) where T : unmanaged, IPacket {
        public void Write(BinaryWriter writer) {
            Packet<T>.Write(writer, in inst);
        }

        public static T Read(BinaryReader reader) {
            return Packet<T>.Read(reader);
        }
    }
}
