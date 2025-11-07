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
        // for (int x = 0; x < Main.maxTilesX; x++) {
        //     for (int y = 0; y < Main.maxTilesY; y++) {
        //         Main.tile[x, y].ClearEverything();
        //     }
        // }
        // InitialShoalsSurfacePass.GenTest();
        base.OnEnter();
    }

    public override List<GenPass> Tasks => new List<GenPass>() {
        new SubworldGenerationPass(progress => {
            progress.Message = "Spawning Seamap";

            Main.worldSurface = Main.maxTilesY - 42;
            Main.rockLayer = Main.maxTilesY;
        }),
        new InitialShoalsSurfacePass("Initial Shoals Block Placement", 1.0f), 
        new SmoothPass("smoothy", 0.4f), 
        new FillWaterPass("Fill It Up!", 0.3f)
    };
    
    public override void OnLoad() {
        //SubworldSystem.noReturn = true;
    }

    public override void Update() {
        Liquid.UpdateLiquid();
        if(Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z) && !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z))
            Main.NewText(InitialShoalsSurfacePass.GetMoundHeight((int)(Main.LocalPlayer.Center.X / 16), 40, 60));

        if(Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X) &&
           !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X)) {
            for (int x = 0; x < Main.maxTilesX; x++) {
                for (int y = 0; y < Main.maxTilesY; y++) {
                    Main.tile[x, y].ClearEverything();
                }
            }
            InitialShoalsSurfacePass.GenTest();
            SmoothPass.SmoothenWorld();
            FillWaterPass.FillRegionWithWater(Main.maxTilesX, Main.maxTilesY - 360, new Vector2(0, 360));
        }
        base.Update();
    }
}