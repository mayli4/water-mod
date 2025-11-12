using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WaterMod.Common.Networking;

//credits to LolXD87 for the gist draft that inspired this implementation: https://gist.github.com/LoLXD8783/374fa5d35f231436f86a1eac4af01899

internal partial class NetworkingHandler {
    private static readonly Dictionary<Type, PacketTypeData> _typeToMetadata = [];
    private static readonly List<PacketTypeData> _packetTypeDataTable = [];

    public static void SendPacket<T>(in T data, int toClient = -1, int ignoreClient = -1) where T : unmanaged, IPacket {
        if(!_typeToMetadata.TryGetValue(typeof(T), out PacketTypeData id))
            throw new ArgumentException($"{typeof(T).Name} is not registered packet type! Is it marked with the [Packet] attribute?");

        var packet = ModContent.GetInstance<ModImpl>().GetPacket();

        packet.Write(id.Id);
        Packet<T>.Write(packet, in data);

        packet.Send(toClient, ignoreClient);
    }

    public static void HandlePacket(BinaryReader reader, int whoAmI) {
        ushort id = reader.ReadUInt16();
        if(id > _packetTypeDataTable.Count) {
            ModContent.GetInstance<ModImpl>().Logger.Debug($"Invalid packet id of {id} received!");
            return;
        }

        _packetTypeDataTable[id].InvokeReceive.Invoke(reader, whoAmI);
    }

    public static void RegisterPacketType<T>() where T : unmanaged, IPacket {
        ref PacketTypeData entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_typeToMetadata, typeof(T), out bool exists);
        if(!exists) {
            var data = new PacketTypeData(checked((ushort)_packetTypeDataTable.Count), (br, from) => Packet<T>.Receive(Packet<T>.Read(br), from));
            entry = data;
            _packetTypeDataTable.Add(entry);
        }
    }

    record struct PacketTypeData(ushort Id, Action<BinaryReader, int> InvokeReceive);
}