using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using SubworldLibrary;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapPlayer : ModPlayer {
    
    [UsedImplicitly]
    [SubscribesTo<ModPlayerHooks.PreUpdate>]
    static void RestrictMovement(ModPlayerHooks.PreUpdate.Original orig, ModPlayer player) {
        orig();
        
        if (!SubworldSystem.IsActive<SeamapSubworld>())
            return;

        player.Player.position = player.Player.oldPosition;

        player.Player.position.X = 0;
        player.Player.position.Y = 0;

        player.Player.fallStart = (int)(player.Player.position.Y / 16f);
    }
}