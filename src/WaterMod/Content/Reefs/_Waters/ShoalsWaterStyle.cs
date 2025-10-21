﻿namespace WaterMod.Content.Reefs;

public class ShoalsWaterStyle : ModWaterStyle {
    public override string Texture => Textures.Waters.KEY_ShoalsWaterStyle;

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
    public override string Texture => Textures.Waters.KEY_ShoalsWaterfallStyle;
    
    public override void AddLight(int i, int j) 
        => Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), Color.Cyan.ToVector3() * 0.5f);
}