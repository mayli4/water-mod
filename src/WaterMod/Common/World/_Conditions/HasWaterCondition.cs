using Terraria.ID;
using Terraria.WorldBuilding;

namespace WaterMod.Common.World;

/// <summary>
///     Provides a <see cref="GenCondition" /> which checks whether a tile has water or not.
/// </summary>
internal sealed class HasWaterCondition : GenCondition
{
    protected override bool CheckValidity(int x, int y)
    {
        return _tiles[x, y].LiquidAmount > 0 && _tiles[x, y].LiquidType == LiquidID.Water;
    }
}