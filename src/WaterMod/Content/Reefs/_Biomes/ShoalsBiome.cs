using SubworldLibrary;
using Terraria.Graphics.Capture;

namespace WaterMod.Content;

internal sealed class ShoalsBiome : ModBiome {
    public override ModWaterStyle WaterStyle => ModContent.GetModWaterStyle(ModContent.GetInstance<ShoalsWaterStyle>().Slot);
    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => Assets.Sound.Music.SurfaceReefs.Slot;

    public override string BestiaryIcon => Assets.Textures.UI.ShoalsBestiary.KEY;

    public override bool IsBiomeActive(Player player) {
        return SubworldSystem.IsActive<ReefsSubworld>();
    }

    public override void SpecialVisuals(Player player, bool isActive) {
        if(isActive) {
            Main.bgStyle = Main.oceanBG;
            Main.rockLayer = Main.maxTilesY;
        }

        base.SpecialVisuals(player, isActive);
    }
}