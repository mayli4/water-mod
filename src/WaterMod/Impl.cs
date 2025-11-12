global using Terraria.ModLoader;
global using Terraria;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
using Daybreak.Common.Features.Authorship;
using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.ModPanel;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using SubworldLibrary;
using Terraria.Localization;
using WaterMod.Common.Networking;
using WaterMod.Content.Reefs;
using WaterMod.Content.Seamap;

namespace WaterMod;

partial class ModImpl : IHasCustomAuthorMessage {
    public ModImpl() {
        MusicAutoloadingEnabled = false;
        CloudAutoloadingEnabled = false;
    }
    
    public string GetAuthorText() => AuthorText.GetAuthorTooltip(this, Mods.WaterMod.UI.ModIcon.AuthorHeader.GetTextValue());

    public override void HandlePacket(System.IO.BinaryReader reader, int whoAmI) => NetworkingHandler.HandlePacket(reader, whoAmI);

#if DEBUG
    [UsedImplicitly]
    [SubscribesTo<ModSystemHooks.PostUpdateEverything>(Side = ModSide.Client)]
    static void DebugKeys(ModSystemHooks.PostUpdateEverything.Original orig, ModSystem self) {
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
#endif
}