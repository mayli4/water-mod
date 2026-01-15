using SubworldLibrary;

namespace WaterMod.Content;

public sealed class ShipyardBiome : ModBiome {
    public override SceneEffectPriority Priority { get; } = SceneEffectPriority.BiomeHigh;

    public override bool IsBiomeActive(Player player) {
        return player.ZoneBeach && player.position.X / 16f < Main.maxTilesX / 2f && !SubworldSystem.AnyActive();
    }

    public override void SpecialVisuals(Player player, bool isActive) {
        base.SpecialVisuals(player, isActive);

        if(isActive /* && !ShipyardGenerationSystem.Repaired */) {
            //player.ZoneGraveyard = true;
        }
    }
}