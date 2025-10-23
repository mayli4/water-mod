using Terraria.WorldBuilding;

namespace WaterMod.Common.World;

/// <summary>
///     Provides a <see cref="GenCondition" /> which checks whether a tile has a tile or not.
/// </summary>
internal sealed class HasTileCondition : GenCondition {
    public override bool CheckValidity(int x, int y) {
        return _tiles[x, y].HasTile;
    }
}