using Daybreak.Common.Features.Hooks;
using SubworldLibrary;
using Terraria;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapHooks {
    [OnLoad]
    static void SubscribeToHooks() {
        On_Player.ItemCheck_CheckCanUse += On_PlayerOnItemCheck_CheckCanUse;
        //prevent player drawing
        On_Main.DrawPlayers_AfterProjectiles += On_MainOnDrawPlayers_AfterProjectiles;
    }

    [OnUnload]
    static void UnsubscribeFromHooks() {
        On_Player.ItemCheck_CheckCanUse -= On_PlayerOnItemCheck_CheckCanUse;
        On_Main.DrawPlayers_AfterProjectiles -= On_MainOnDrawPlayers_AfterProjectiles;
    }
    
    static bool On_PlayerOnItemCheck_CheckCanUse(On_Player.orig_ItemCheck_CheckCanUse orig, Player self, Item item) {
        if (SubworldSystem.IsActive<SeamapSubworld>() && self.whoAmI == Main.myPlayer)
            return false;

        return orig(self, item);
    }

    static void On_MainOnDrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        if (SubworldSystem.IsActive<SeamapSubworld>())
            return;

        orig(self);
    }
}