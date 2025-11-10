using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WaterMod.Common.Networking;
using WaterMod.Generator;

namespace WaterMod.Common.Networking;

delegate void ReceivePacket<T>(in T packet, int fromWho);

internal class Packet<T> where T : unmanaged, IPacket {
    public static event ReceivePacket<T>? OnReceive;

    public static void Recieve(in T data, int fromWho) {
        OnReceive?.Invoke(in data, fromWho);
    }

    [SkipLocalsInit]
    public static unsafe void Write(BinaryWriter binaryWriter, in T data) {
        Span<byte> buf = stackalloc byte[sizeof(T)];
        MemoryMarshal.Write(buf, in data);
        binaryWriter.Write(buf);
    }

    [SkipLocalsInit]
    public static unsafe T Read(BinaryReader binaryReader) {
        Span<byte> buf = stackalloc byte[sizeof(T)];
        binaryReader.Read(buf);
        return MemoryMarshal.Read<T>(buf);
    }
}