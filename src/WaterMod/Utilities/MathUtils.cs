namespace WaterMod.Utilities;

internal static class MathUtils {
    public static float InverseLerp(float from, float to, float x, bool clamped = true) {
        float interpolant = (x - from) / (to - from);
        if (!clamped)
            return interpolant;

        return Saturate(interpolant);
    }

    public static float Saturate(float x) {
        if (x > 1f)
            return 1f;
        if (x < 0f)
            return 0f;
        return x;
    }
}