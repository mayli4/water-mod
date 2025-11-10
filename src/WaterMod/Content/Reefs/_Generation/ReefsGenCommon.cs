using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using WaterMod.Utilities;

namespace WaterMod.Content.Reefs;

internal class ReefsGenCommon {
    private static FastNoiseLite _noiseGenerator;
    
    [OnLoad]
    [UsedImplicitly]
    static void Load() {
        _noiseGenerator = new FastNoiseLite();
        _noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    }
    
    public static float[] GenerateNoiseStrip(int width, int seed, float amplitude, float frequencyX, float frequencyY, FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin) {
        _noiseGenerator.SetSeed(seed);
        _noiseGenerator.SetNoiseType(noiseType);
        _noiseGenerator.SetFrequency(frequencyX);
        
        int yOffset = WorldGen.genRand.Next(0, 1000);

        float[] noiseStrip = new float[width];
        for (int i = 0; i < width; i++) {
            noiseStrip[i] = _noiseGenerator.GetNoise(i, yOffset) * amplitude;
        }
        return noiseStrip;
    }
}