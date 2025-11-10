using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WaterMod.Generator;

namespace WaterMod.Common.Networking;

internal partial class NetworkingHandler : ILoadable {
    private static readonly Dictionary<Type, PacketTypeData> _typeToMetadata = [];
    private static List<PacketTypeData> _packetTypeDataTable = [];

    public void Load(Mod mod) {

    }

    public void Unload() {

    }

    public static void SendPacket<T>(in T data, int toClient = -1, int ignoreClient = -1) where T : unmanaged, IPacket {
        var packet = ModContent.GetInstance<ModImpl>().GetPacket();
        Packet<T>.Write(packet, in data);
        packet.Send(toClient, ignoreClient);
    }

    public static void RegisterPacketType<T>() where T : unmanaged, IPacket {
        ref PacketTypeData entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_typeToMetadata, typeof(T), out bool exists);
        if(!exists) {
            var data = new PacketTypeData(checked((ushort)_packetTypeDataTable.Count), (br, from) => Packet<T>.Recieve(Packet<T>.Read(br), from));
            entry = data;
            _packetTypeDataTable.Add(entry);
        }
    }

    record struct PacketTypeData(ushort Id, Action<BinaryReader, int> InvokeReceive);
}
