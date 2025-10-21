using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Input;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WaterMod.Content.Reefs;
using WaterMod.Content.Seamap;

namespace WaterMod;

partial class WaterModImpl : Mod;

#if DEBUG
internal sealed class Debug : ILoadable {
    [SubscribesTo<ModSystemHooks.PostUpdateEverything>(Side = ModSide.Client)]
    void DebugKeys(ModSystemHooks.PostUpdateEverything.Original orig, ModSystem self) {
        orig();

        #region seamap
        if(!SubworldSystem.IsActive<SeamapSubworld>() && Main.keyState.IsKeyDown(Keys.P) && Main.keyState.IsKeyDown(Keys.LeftShift) ) {
            SubworldSystem.Enter<SeamapSubworld>();
            Main.NewText(Language.GetText("Mods.WaterMod.SubworldEnterText.SeamapEnter"));
        }
        
        if(SubworldSystem.IsActive<SeamapSubworld>() && Main.keyState.IsKeyDown(Keys.O) && Main.keyState.IsKeyDown(Keys.LeftShift) ) {
            SubworldSystem.Exit();
            Main.NewText(Language.GetText("Mods.WaterMod.SubworldEnterText.SeamapEnter"));
        }
        #endregion

        #region reefs

        if(!SubworldSystem.IsActive<ReefsSubworld>() && Main.keyState.IsKeyDown(Keys.L) && Main.keyState.IsKeyDown(Keys.LeftShift) ) {
            SubworldSystem.Enter<ReefsSubworld>();
            Main.NewText(Language.GetText("Mods.WaterMod.SubworldEnterText.SeamapEnter"));
        }
        
        if(SubworldSystem.IsActive<ReefsSubworld>() && Main.keyState.IsKeyDown(Keys.K) && Main.keyState.IsKeyDown(Keys.LeftShift) ) {
            SubworldSystem.Exit();
            Main.NewText(Language.GetText("Mods.WaterMod.SubworldEnterText.SeamapEnter"));
        }

        #endregion

    }

    public void Load(Mod mod) {
        
    }

    public void Unload() {
        
    }
}
#endif