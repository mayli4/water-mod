using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterMod.Networking;

internal class PacketMessage<T> where T : IPacket
{


    public static void Send(in T packet) {

    }
}
