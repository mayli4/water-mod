using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace WaterMod.Content.Reefs;

internal sealed class ShoalsBiome : ModBiome {
    public override ModWaterStyle WaterStyle => ModContent.GetModWaterStyle(ModContent.GetInstance<ShoalsWaterStyle>().Slot);
    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => base.BackgroundColor;

    public override bool IsBiomeActive(Player player) {
        return SubworldSystem.IsActive<ReefsSubworld>();
    }

    public override void SpecialVisuals(Player player, bool isActive) {
        if (isActive) {
            Main.bgStyle = Main.oceanBG;
            Main.rockLayer = Main.maxTilesY;
        }

        base.SpecialVisuals(player, isActive);
    }
}