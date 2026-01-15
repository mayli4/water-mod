using Terraria.DataStructures;

namespace WaterMod.Utilities;

internal static class TileUtils {
    public static T FindTileEntity<T>(int i, int j, int width, int height, int sheetSquare = 16) where T : ModTileEntity {
        Tile tile = Main.tile[i, j];
        int x = i - tile.TileFrameX % (width * sheetSquare) / sheetSquare;
        int y = j - tile.TileFrameY % (height * sheetSquare) / sheetSquare;
        int type = ModContent.GetInstance<T>().Type;
        if (!TileEntity.ByPosition.TryGetValue(new Point16(x, y), out var value) || value.type != type) {
            return null;
        }

        return (T)value;
    }
}