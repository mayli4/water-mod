using Daybreak.Common.Features.Hooks;
using SubworldLibrary;
using Terraria.Graphics.Light;

namespace WaterMod.Content.Reefs;

internal sealed class ReefsHooks {
    [OnLoad]
    static void SubscribeToHooks() {
        On_TileLightScanner.GetTileLight += (On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int i, int j, out Vector3 color) => {
            orig(self, i, j, out color);

            if(SubworldSystem.IsActive<ReefsSubworld>()) {
                if (!Main.tile[i, j].HasTile)
                    color += new Vector3(0.15f, 0.175f, 0.25f) * 0.5f;
            }
        };
    }
}