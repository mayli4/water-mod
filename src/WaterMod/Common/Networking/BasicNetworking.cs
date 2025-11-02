using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WaterMod.Common.Networking;

//TODO replace this entirely. using SR's solution until we can workshop a much better solution ourselves in the future (med priority)
// https://github.com/GabeHasWon/SpiritReforged/tree/master/Common/Multiplayer

internal class MultiplayerHandler : ILoadable
{
    internal static readonly Dictionary<byte, PacketData> PacketTypes = [];

    /// <summary> Loads all data definitions into a static lookup (<see cref="PacketTypes"/>). Must be ordered consistently between clients. </summary>
    public void Load(Mod mod)
    {
        byte count = 0;
        var packets = mod.Code.GetTypes().Where(x => x.IsSubclassOf(typeof(PacketData)) && !x.IsAbstract).OrderBy(x => x.Name, StringComparer.InvariantCulture);

        foreach (var packet in packets)
        {
            PacketTypes.Add(count, (PacketData)Activator.CreateInstance(packet));
            count++;
        }
    }

    public void Unload() => PacketTypes.Clear();

    public static void HandlePacket(BinaryReader reader, int whoAmI)
    {
        byte id = reader.ReadByte();

        if (PacketTypes.TryGetValue(id, out var data))
        {
            if (data.Log)
                ModContent.GetInstance<ModImpl>().Logger.Debug("[Synchronization] Reading incoming: " + data.GetType().Name);

            data.OnReceive(reader, whoAmI);
        }
        else
            ModContent.GetInstance<ModImpl>().Logger.Debug("[Synchronization] Invalid data id: " + id);
    }
}

/// <summary> Encapsulates custom multiplayer data to send interchangeably between server and clients.<br/>
/// Must include a parameterless constructor for initialization purposes. </summary>
internal abstract class PacketData
{
    /// <summary>
    /// Whether this packet will be logged. True by default.
    /// </summary>
    public virtual bool Log => true;

    /// <summary> This must be called after creating a new packet instance in order for it to be sent. </summary>
    public void Send(int toClient = -1, int ignoreClient = -1)
    {
        byte id = MultiplayerHandler.PacketTypes.Where(x => x.Value.GetType() == GetType()).First().Key;

        var packet = ModContent.GetInstance<ModImpl>().GetPacket();
        packet.Write(id);
        OnSend(packet);
        packet.Send(toClient, ignoreClient);
    }

    /// <summary> Use <paramref name="modPacket"/> to write data here (usually fields assigned by the constructor).<br/>
    /// Sending is automatic and should not be done here. </summary>
    public abstract void OnSend(ModPacket modPacket);
    /// <summary> Read your packet data here. <br/> Remember that only <paramref name="reader"/> should be used to get variable data, not fields. </summary>
    /// <param name="whoAmI">The index of player this message is from. Only relevant for server code.</param>
    public abstract void OnReceive(BinaryReader reader, int whoAmI);
}