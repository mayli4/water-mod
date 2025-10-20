float bayer_2x2(float2 a) {
    a = floor(a);
    return frac(a.x / 2.0f + a.y * a.y * 0.75f);
}