using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.WorldBuilding;
using WaterMod.Common.Subworlds;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapSubworld : Subworld {
    public override int Width => 300;

    public override int Height => 300;

    public override string Name => "Sea";

    public override List<GenPass> Tasks => new List<GenPass>() {
        new SubworldGenerationPass(progress => {
            progress.Message = "Spawning Seamap";

            Main.worldSurface = Main.maxTilesY - 42;
            Main.rockLayer = Main.maxTilesY;
        })
    };

    public override void DrawMenu(GameTime gameTime) {
        var bar = new UIGenProgressBar();
        bar.Draw(Main.spriteBatch);
    }
}
