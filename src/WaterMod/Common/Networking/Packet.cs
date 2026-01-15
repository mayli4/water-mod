using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WaterMod.Common.Networking;

/// <summary>
///     A delegate for handling a received packet of type <typeparamref name="T"/>.
/// </summary>
///     <typeparam name="T">The type of the packet being received.</typeparam>
///     <param name="packet">The deserialized packet data.</param>
///     <param name="fromWho">The client id from whom the packet was received.</param>
internal delegate void ReceivePacket<T>(in T packet, int fromWho);

/// <summary>
///     Contains methods for writing and reading packets to and from a binary stream, and manages event subscriptions for packet receiving.
/// </summary>
///     <typeparam name="T">A struct that represents the packet.</typeparam>
internal class Packet<T> where T : unmanaged, IPacket
{
    public static event ReceivePacket<T>? OnReceive;

    public static void Receive(in T data, int fromWho)
    {
        OnReceive?.Invoke(in data, fromWho);
    }

    [SkipLocalsInit]
    public static unsafe void Write(BinaryWriter binaryWriter, in T data)
    {
        Span<byte> buf = stackalloc byte[sizeof(T)];
        MemoryMarshal.Write(buf, in data);
        binaryWriter.Write(buf);
    }

    [SkipLocalsInit]
    public static unsafe T Read(BinaryReader binaryReader)
    {
        Span<byte> buf = stackalloc byte[sizeof(T)];
        binaryReader.Read(buf);
        return MemoryMarshal.Read<T>(buf);
    }
}