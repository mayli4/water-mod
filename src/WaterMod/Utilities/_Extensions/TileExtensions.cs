using Terraria.ObjectData;

namespace WaterMod.Utilities;

internal static class TileExtensions {
        /// <summary>
    ///     Checks whether a tile is solid or not, including tiles that are only solid on their top surface.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <returns><c>true</c> if the tile is solid; otherwise, <c>false</c>.</returns>
    public static bool IsSolid(this Tile tile)
    {
        return tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]);
    }

    /// <summary>
    ///     Checks whether a tile has a specific type or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the tile has the specified type; otherwise, <c>false</c>.</returns>
    public static bool HasTileType(this Tile tile, int type)
    {
        return tile.HasTile && tile.TileType == type;
    }

    /// <summary>
    ///     Checks whether a tile has a specific type and style or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <param name="type">The type to check.</param>
    /// <param name="style">The style to check.</param>
    /// <returns><c>true</c> if the tile has the specified type and style; otherwise, <c>false</c>.</returns>
    public static bool HasTileType(this Tile tile, int type, int style)
    {
        return tile.HasTile && tile.TileType == type && TileObjectData.GetTileStyle(tile) == style;
    }

    /// <summary>
    ///     Checks whether a tile has a specific liquid type or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <param name="type">The liquid type to check.</param>
    /// <returns><c>true</c> if the tile has the specified liquid type; otherwise, <c>false</c>.</returns>
    public static bool HasLiquidType(this Tile tile, int type)
    {
        return tile.LiquidType == type;
    }

    /// <summary>
    ///     Checks whether a tile has a specific liquid amount or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <param name="amount">The liquid amount to check.</param>
    /// <returns><c>true</c> if the tile has the specified liquid amount; otherwise, <c>false</c>.</returns>
    public static bool HasLiquidAmount(this Tile tile, byte amount)
    {
        return tile.LiquidAmount >= amount;
    }

    /// <summary>
    ///     Checks whether a tile has any liquid amount or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <returns><c>true</c> if the tile has any liquid amount; otherwise, <c>false</c>.</returns>
    public static bool HasAnyLiquidAmount(this Tile tile)
    {
        return tile.LiquidAmount > 0;
    }

    /// <summary>
    ///     Checks whether a tile is at full liquid capacity or not.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <returns><c>true</c> if the tile is at full liquid capacity; otherwise, <c>false</c>.</returns>
    public static bool HasFullLiquidAmount(this Tile tile)
    {
        return tile.LiquidAmount >= byte.MaxValue;
    }
}