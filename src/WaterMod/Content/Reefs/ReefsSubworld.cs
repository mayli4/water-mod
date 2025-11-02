using ReLogic.Content;
using SubworldLibrary;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.Shaders;
using Terraria.WorldBuilding;
using WaterMod.Common.Rendering;
using WaterMod.Common.Subworlds;
using WaterMod.Content.Achievements;

namespace WaterMod.Content.Reefs;

internal sealed class ReefsSubworld : Subworld {
    public override int Width => 1000;

    public override int Height => 1200;

    public override string Name => "Coral Reefs";
    
    public override void OnEnter() {
        if (ModContent.GetInstance<ReefsAchievement>() is { } achievement) {
            achievement.SubworldEnteredCondition.Complete();
        }
        base.OnEnter();
    }

    public override List<GenPass> Tasks => new List<GenPass>() {
        new SubworldGenerationPass(progress => {
            progress.Message = "Spawning Seamap";

            Main.worldSurface = Main.maxTilesY - 42;
            Main.rockLayer = Main.maxTilesY;
        }),
        new InitialShoalsSurfacePass("Initial Shoals Block Placement", 1.0f), 
    };
    
    public override void OnLoad() {
        SubworldSystem.noReturn = true;
    }

    public override void Update() {
        Liquid.UpdateLiquid();
        base.Update();
    }
}