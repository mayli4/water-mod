using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace WaterMod.Content.Reefs;

internal class SmoothPass(string name, double loadWeight) : GenPass(name, loadWeight) {
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        SmoothenWorld();
    }

    public static void SmoothenWorld() {
        for(int x = 5; x < Main.maxTilesX - 5; x++) {
            for(int y = 5; y < Main.maxTilesY - 5; y++) {
                SlopeType oldSlope = Framing.GetTileSafely(x, y).Slope;
                Tile.SmoothSlope(x, y);
                Tile t = Framing.GetTileSafely(x, y);

                if(t.Slope != oldSlope) {
                    t.Get<LiquidData>().Amount = 255;

                    t = Framing.GetTileSafely(x, y - 1);
                    t.Get<LiquidData>().Amount = 255;

                    t = Framing.GetTileSafely(x, y + 1);
                    t.Get<LiquidData>().Amount = 255;
                }
            }
        }
    }
}