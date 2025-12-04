using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace WaterMod.Content.Reefs;

internal sealed class InitialShoalsSurfacePass(string name, double loadWeight) : GenPass(name, loadWeight) {
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "Shoveling shoals...";
        GenTest();
    }
    public static Point TrenchBottom;
    public static void GenTest() {
        int trenchEntranceX = WorldGen.genRand.Next(Main.maxTilesX / 5 * 2, Main.maxTilesX / 5 * 3);
        int deepestPointInTrench = 0;
        for(int x = 0; x < Main.maxTilesX; x++) {
            float topY = GetSurfaceLevelThreshold(x, 12, Main.maxTilesY / 2, 0.2f, amplitude: 1);
            topY += -GetSurfaceLevelThreshold(x, 8, Main.maxTilesY / 2, 0.1f, 15) * 0.25f;
            int moundRelativeX = (x) % (Main.maxTilesX / 2);
            float moundY = Logistic(MathF.Abs(Main.maxTilesX / 4 - moundRelativeX), Main.maxTilesX / 8, 0.05f)*(MathF.Sin(x*MathF.PI*2)*0.5f+1f)*80+340;
            moundY += GetSurfaceLevelThreshold(x, 6, 0, 0.2f, 1.5f);
            float height = MathF.Min(topY, moundY);
            for(int y = 0; y < Main.maxTilesY; y++) {
                if(y > height) {
                    ushort type = (ushort)ModContent.TileType<CoralsandTile>();
                    if(y > height * 1.2f)
                        type = TileID.Stone;
                    Main.tile[x, y].ResetToType(type);
                }
                //float trenchX = x + (MathF.Sin(y * MathF.PI* 1000) * 0.5f + 0.5f) * 50;
                float xMod = 1.5f;
                //float xMod = 1.5f + MathF.Abs(y-Main.maxTilesY/2)/(Main.maxTilesY/4);
                //xMod = MathF.Pow(xMod, 4);
                float distFromTrench = ((x - trenchEntranceX) * (x - trenchEntranceX))*xMod + ((y - height) * (y - height))*0.15f;
                if(distFromTrench < 9600 && WorldGen.SolidTile(x, y)) {
                    Main.tile[x, y].ClearTile();
                    if(y > deepestPointInTrench)
                        deepestPointInTrench = y;
                }
            }
        }
        TrenchBottom = new(trenchEntranceX, deepestPointInTrench);
    }

    public static float GetMoundHeight(int x, int moundLength, int frequency, int offsetY = 0) {
        x = (x % frequency) - 20;
        return -hyperbole(x) + hyperbole(x - moundLength);

        float hyperbole(int x) {
            return MathF.Sqrt(1 + (x * x)) - MathF.Sqrt(1 + MathF.Pow(2 + (x * x), 2));
        }
    }
    public static float Logistic(float value, float midpoint = 0f, float steepness = 1f) {
        float expo = MathF.Exp(-steepness * (value - midpoint));
        return 1f / (1f + expo);
    }

    public static float GetSurfaceLevelThreshold(int x, int octaves, int offsetY = 0, float frequency = 1, float amplitude = 1) {
        float threshold = 0f;
        for(int i = 0; i < octaves; i++) {
            //(cos(pow(uv.x * uResolution.x, 1) / (i + 1) * freq) + i) * amplitude;
            float topY = (MathF.Cos(x / (i + 1) * frequency) + i) * amplitude;
            threshold += topY;
        }
        return threshold + offsetY;
    }
}
public class DigCavesPass(string name, double loadWeight) : GenPass(name, loadWeight) {
    readonly LinkedList<CaveNode> _nodes = [];
    private const int MAX_CAVES = 30;
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        AddCaveNodes();        
    }
    public static void AddCaveNodes() {

    }
    public static void ShapeCaves() {

    }
    public readonly record struct CaveNode(Point Center, int Size) {

    }
}
public class FillWaterPass(string name, double loadWeight) : GenPass(name, loadWeight) {
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        => FillRegionWithWater(Main.maxTilesX, Main.maxTilesY - 360, new Vector2(0, 360));

    public static void FillRegionWithWater(int width, int height, Vector2 startingPoint) {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                Framing.GetTileSafely(i + (int)startingPoint.X, j + (int)startingPoint.Y).LiquidType = LiquidID.Water;
                Framing.GetTileSafely(i + (int)startingPoint.X, j + (int)startingPoint.Y).LiquidAmount = 255;
                WorldGen.SquareTileFrame(i + (int)startingPoint.X, j + (int)startingPoint.Y);
                if(Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.sendWater(i + (int)startingPoint.X, j + (int)startingPoint.Y);
            }
        }
    }
}