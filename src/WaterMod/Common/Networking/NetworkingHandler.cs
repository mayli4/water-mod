using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WaterMod.Common.Networking;

//credits to LolXD87 for the gist draft that inspired this implementation: https://gist.github.com/LoLXD8783/374fa5d35f231436f86a1eac4af01899

/// <summary>
///     Handles the sending/receiving of custom packets. This manages the registration of packets, and dispatches incoming packets accordingly.
/// </summary>
internal partial class NetworkingHandler {
    private static readonly Dictionary<Type, PacketTypeData> type_to_metadata = [];
    private static readonly List<PacketTypeData> packet_type_data_table = [];

    /// <summary>
    ///     Sends a custom packet to a specific client or all clients.
    /// </summary>
    ///     <param name="data">The type of packet data to send.</param>
    ///     <param name="toClient">The client id to send the packet to. Use -1 to send to all clients.</param>
    ///     <param name="ignoreClient">The client id to ignore when sending this packet. Use -1 to ignore no clients.</param>
    ///     <typeparam name="T">The type of the packet to send.</typeparam>
    ///     <exception cref="ArgumentException">Thrown if the packet of type <typeparamref name="T"/> has not been registered. Ensure it is marked with the <c>[Packet]</c> attribute.</exception>
    public static void SendPacket<T>(in T data, int toClient = -1, int ignoreClient = -1) where T : unmanaged, IPacket {
        if(!type_to_metadata.TryGetValue(typeof(T), out PacketTypeData id))
            throw new ArgumentException($"{typeof(T).Name} is not registered packet type! Is it marked with the [Packet] attribute?");

        var packet = ModContent.GetInstance<ModImpl>().GetPacket();

        packet.Write(id.Id);
        Packet<T>.Write(packet, in data);

        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    ///     Handles an incoming packet by reading its id and dispatching it to its appropriate handler.
    /// </summary>
    ///     <param name="reader">The <see cref="BinaryReader"/> containing the packet data, starting from the packet id.</param>
    ///     <param name="whoAmI">The client id, from whom the packet was recieved.</param>
    public static void HandlePacket(BinaryReader reader, int whoAmI) {
        ushort id = reader.ReadUInt16();
        if(id > packet_type_data_table.Count) {
            ModContent.GetInstance<ModImpl>().Logger.Debug($"Invalid packet id of {id} received!");
            return;
        }

        packet_type_data_table[id].InvokeReceive.Invoke(reader, whoAmI);
    }

    /// <summary>
    ///     Registers a packet type. This method should not be used manually, as it is called by the source generator.
    /// </summary>
    /// <typeparam name="T">The struct type to register as a packet.</typeparam>
    public static void RegisterPacketType<T>() where T : unmanaged, IPacket {
        ref PacketTypeData entry = ref CollectionsMarshal.GetValueRefOrAddDefault(type_to_metadata, typeof(T), out bool exists);
        if(!exists) {
            var data = new PacketTypeData(checked((ushort)packet_type_data_table.Count), (br, from) => Packet<T>.Receive(Packet<T>.Read(br), from));
            entry = data;
            packet_type_data_table.Add(entry);
        }
    }

    /// <summary>
    ///     Represents data for a registered packet, including its unique id and a delegate to invoke its handler.
    /// </summary>
    ///     <param name="Id">The unique ID assigned to this packet type.</param>
    ///     <param name="InvokeReceive">A delegate that, when invoked, reads the packet from a <see cref="BinaryReader"/> and dispatches it to the appropriate <see cref="Packet{T}.OnReceive"/> event.</param>
    record struct PacketTypeData(ushort Id, Action<BinaryReader, int> InvokeReceive);
}