using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using SubworldLibrary;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using WaterMod.Utilities;
using WaterMod.Generator;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapPlayer : ModPlayer {
    public override void PreUpdate() {
        if (!SubworldSystem.IsActive<SeamapSubworld>())
            return;

        Player.position = Player.oldPosition;

        Player.position.X = 0;
        Player.position.Y = 0;

        Player.fallStart = (int)(Player.position.Y / 16f);
    }
}