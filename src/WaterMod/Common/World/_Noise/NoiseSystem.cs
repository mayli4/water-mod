using Daybreak.Common.Features.Hooks;
using WaterMod.Utilities;

namespace WaterMod.Common.World;

internal static class NoiseSystem
{
    public static FastNoiseLite Noise;

    [OnLoad]
    static void InitNoise()
    {
        Noise = new(Main.rand.Next());
    }

    public static float Perlin(float x, float y) => Noise.GetNoise(x, y);
}