using System;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace WaterMod.Content.Reefs;

internal sealed class InitialShoalsSurfacePass(string name, double loadWeight) : GenPass(name, loadWeight) {
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "Shoveling shoals...";
        GenTest();  
    }
    
    public static void GenTest() {
        for(int x = 0; x < Main.maxTilesX; x++) {
            float topY = InitialShoalsSurfacePass.GetSurfaceLevelThreshold(x, 12, Main.maxTilesY / 2, 0.2f, amplitude: 1);
            topY += -InitialShoalsSurfacePass.GetSurfaceLevelThreshold(x, 8, Main.maxTilesY / 2, 0.15f, 15) * 0.25f;
            for(int y = 0; y < Main.maxTilesY; y++) {
                if(y > topY) {
                    Main.tile[x, y].ResetToType((ushort)ModContent.TileType<CoralsandTile>());
                }
            }
        }
    }
    public static float GetMoundHeight(int x, int moundLength, int frequency, int offsetY = 0) {
        x = (x % frequency)-20;
        return -Hyperbole(x) + Hyperbole(x-moundLength);
        
        float Hyperbole(int x) {
            return MathF.Sqrt(1 + (x * x)) - MathF.Sqrt(1 + MathF.Pow(2 + (x * x), 2));
        }
    }
    public static float GetSurfaceLevelThreshold(int x, int octaves, int offsetY = 0, float frequency = 1, float amplitude = 1) {
        float threshold = 0f;
        for (int i = 0; i < octaves; i++) {
            //(cos(pow(uv.x * uResolution.x, 1) / (i + 1) * freq) + i) * amplitude;
            float topY = (MathF.Cos(x / (i + 1) * frequency) + i) * amplitude;
            threshold += topY;
        }
        return threshold  + offsetY;
    }
}

public class FillWaterPass : GenPass
{
    public FillWaterPass(string name, double loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) 
        => FillRegionWithWater(Main.maxTilesX, Main.maxTilesY - 420, new Vector2(0, 420));
    
    public static void FillRegionWithWater(int width, int height, Vector2 startingPoint)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Framing.GetTileSafely(i + (int)startingPoint.X, j + (int)startingPoint.Y).LiquidType = 0; ; // set liquid type 0 is water 1 lava 2 honey 3+ water iirc
                Framing.GetTileSafely(i + (int)startingPoint.X, j + (int)startingPoint.Y).LiquidAmount = 255; // set liquid ammount
                WorldGen.SquareTileFrame(i + (int)startingPoint.X, j + (int)startingPoint.Y, true); // soemthing for astatic voiding the liquid from being static
                if (Main.netMode == NetmodeID.MultiplayerClient) // sync
                {
                    NetMessage.sendWater(i + (int)startingPoint.X, j + (int)startingPoint.Y);
                }
            }
        }
    }
}