namespace WaterMod.Content.Reefs;

internal sealed class ShoalsWaterStyle : ModWaterStyle {
    public override string Texture => Assets.Textures.Waters.ShoalsWaterStyle.KEY;

    public override int ChooseWaterfallStyle() => ModContent.GetInstance<ShoalsWaterStyle>().Slot;

    public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
        r = 1.08f;
        g = 1.08f;
        b = 1.08f;
    }

    public override Color BiomeHairColor() => Color.Cyan;

    public override byte GetRainVariant() => (byte)Main.rand.Next(3);

    public override int GetSplashDust() => 0;

    public override int GetDropletGore() => 0;
}

internal sealed class ShoalWaterfallStyle : ModWaterfallStyle {
    public override string Texture => Assets.Textures.Waters.ShoalsWaterfallStyle.KEY;
    
    public override void AddLight(int i, int j) 
        => Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), Color.Cyan.ToVector3() * 0.5f);
}