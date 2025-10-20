using SubworldLibrary;
using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace WaterMod.Content.Reefs;

internal sealed class ReefsSubworld : Subworld {
    public override int Width { get; }
    public override int Height { get; }
    public override List<GenPass> Tasks { get; }
}