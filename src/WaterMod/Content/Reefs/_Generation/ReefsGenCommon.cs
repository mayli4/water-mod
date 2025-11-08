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
}