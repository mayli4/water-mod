using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using WaterMod.Content.Seamap;

namespace WaterMod.Common.UI;

internal class SeamapUI {
    [UsedImplicitly]
    [OnLoad]
    static void SubscribeToHooks() {
        On_Main.DrawInterface_16_MapOrMinimap += On_MainOnDrawInterface_16_MapOrMinimap;
    }
    
    [UsedImplicitly]
    [OnUnload]
    static void UnsubscribeFromHooks() {
        On_Main.DrawInterface_16_MapOrMinimap -= On_MainOnDrawInterface_16_MapOrMinimap;
    }

    [UsedImplicitly]
    [SubscribesTo<ModSystemHooks.PostUpdateInput>]
    static void KillMinimapInputs(ModSystemHooks.PostUpdateInput.Original orig, ModSystem self) {
        orig();

        if(!SubworldSystem.IsActive<SeamapSubworld>()) return;
        
        PlayerInput.Triggers.Current.MapFull = false;
        PlayerInput.Triggers.Current.MapStyle = false;
    }
    
    [UsedImplicitly]
    [SubscribesTo<ModSystemHooks.ModifyInterfaceLayers>]
    static void InjectCustomMinimap(ModSystemHooks.ModifyInterfaceLayers.Original orig, ModSystem self, List<GameInterfaceLayer> layers) {
        orig(layers); 
        
        if(!SubworldSystem.IsActive<SeamapSubworld>()) return;

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        layers.RemoveAll(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        layers.RemoveAll(layer => layer.Name.Equals("Vanilla: Inventory"));

        if (mouseTextIndex != -1) {
            layers.Insert(
                mouseTextIndex,
                new LegacyGameInterfaceLayer(
                    "SeamapMinimap",
                    delegate {
                        float uiScale = Main.UIScale;

                        int minimapX = (int)(Main.screenWidth - 220 * uiScale);
                        int minimapY = (int)(10 * uiScale);
                        int minimapWidth = (int)(200 * uiScale);
                        int minimapHeight = (int)(150 * uiScale);

                        Main.spriteBatch.Draw(
                            TextureAssets.MagicPixel.Value,
                            new Rectangle(minimapX, minimapY, minimapWidth, minimapHeight),
                            Color.Blue * 0.8f
                        );

                        return true;
                    },
                    InterfaceScaleType.UI
                )
            );
        }
    }
    
    static void On_MainOnDrawInterface_16_MapOrMinimap(On_Main.orig_DrawInterface_16_MapOrMinimap orig, Main self) {
        if(SubworldSystem.IsActive<SeamapSubworld>()) return;

        orig(self);
    }
}