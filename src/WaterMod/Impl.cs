using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Input;
using SubworldLibrary;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WaterMod.Content.Seamap;

namespace WaterMod;

partial class WaterModImpl : Mod;

#if DEBUG
internal sealed class Debug {
    [SubscribesTo<ModSystemHooks.PostUpdateEverything>(Side = ModSide.Client)]
    void DebugKeys(ModSystemHooks.PostUpdateEverything.Original orig, ModSystem self) {
        orig();
        
        if(!SubworldSystem.IsActive<SeamapSubworld>() && Main.keyState.IsKeyDown(Keys.P) && Main.keyState.IsKeyDown(Keys.LeftShift) ) {
            SubworldSystem.Enter<SeamapSubworld>();
            Main.NewText(Language.GetText("Mods.WaterMod.SubworldEnterText.SeamapEnter"));
        }
    }
}
#endif