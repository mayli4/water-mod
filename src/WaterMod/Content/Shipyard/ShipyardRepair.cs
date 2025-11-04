// using Terraria.Audio;
// using Terraria.DataStructures;
// using Terraria.Graphics.Effects;
// using Terraria.Graphics.Shaders;
// using Terraria.ID;
// using Filters = Terraria.GameContent.Bestiary.Filters;
//
// namespace WaterMod.Content.Shipyard;
//
//
// class ShipyardRepairPlayer : ModPlayer
// {
//     private int timeRemaining;
//     public void StartFadeToBlack()
//     {
//         timeRemaining = 300;
//
//         Filters.Scene.Activate("EndlessEscapade:RingFadeToBlack", Main.LocalPlayer.Center);
//     }
//
//     public override void PreUpdate()
//     {
//         if (timeRemaining > 0)
//         {
//             //Apply the opacity
//             Filters.Scene["EndlessEscapade:RingFadeToBlack"].GetShader().UseOpacity(1500f);
//
//             //Play a hammer sound as the boat is being built
//             if (timeRemaining % 60 == 0 && timeRemaining > 0)
//                 SoundEngine.PlaySound(SoundID.Item37, Main.LocalPlayer.Center);
//
//             //Midway through cutscene, build the boat and give it some time
//             if (timeRemaining == 150)
//             {
//                 StructureHelper.API.Generator.GenerateStructure("Assets/Structures/Sailboat.shstruct", new Point16(ShipyardGenerationSystem.SailboatOrigin.X - 22, ShipyardGenerationSystem.SailboatOrigin.Y - 27), EndlessEscapade.Instance);
//
//                 WorldGen.PlaceTile(ShipyardGenerationSystem.SailboatOrigin.X - 22 + 12, ShipyardGenerationSystem.SailboatOrigin.Y - 27 + 25, ModContent.TileType<WoodenShipsWheelTile>(), forced: true);
//             }
//
//             //Just as the fade ends, warp the player onto the boat
//             if (timeRemaining == 60)
//             {
//
//             }
//
//
//             timeRemaining--;
//         }
//
//         //End the cutscene once the fade is gone
//         else
//         {
//             if (Filters.Scene["EndlessEscapade:RingFadeToBlack"].IsActive()) Filters.Scene.Deactivate("EndlessEscapade:RingFadeToBlack");
//         }
//     }
// }
//
// public sealed class ShipyardRepairSystem : ModSystem
// {
//     public override void Load()
//     {
//         Filters.Scene["EndlessEscapade:RingFadeToBlack"] =
//             new Filter(new ScreenShaderData(Assets.Shaders.Fragment.FadeToBlack.Asset, "Ripple"), EffectPriority.VeryHigh);
//     }
//
//     public void StartRepairSequence()
//     {
//         Main.LocalPlayer.GetModPlayer<ShipyardRepairPlayer>().StartFadeToBlack();
//     }
// }