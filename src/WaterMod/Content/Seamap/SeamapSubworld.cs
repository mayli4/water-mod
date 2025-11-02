using ReLogic.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;
using WaterMod.Common.Subworlds;
using WaterMod.Content.Achievements;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapSubworld : Subworld {
    public override int Width => 300;

    public override int Height => 300;

    public override string Name => "Sea";

    public override void OnEnter() {
        if (ModContent.GetInstance<SetSailAchievement>() is { } achievement) {
            achievement.SubworldEnteredCondition.Complete();
        }
        base.OnEnter();
    }

    public override void OnLoad() {
        SubworldSystem.noReturn = true;
    }

    public override List<GenPass> Tasks => new List<GenPass>() {
        new SubworldGenerationPass(progress => {
            progress.Message = "Spawning Seamap";

            Main.worldSurface = Main.maxTilesY - 42;
            Main.rockLayer = Main.maxTilesY;
        })
    };

    public override void DrawMenu(GameTime gameTime) {
        // var bar = new UIGenProgressBar();
        // bar.Draw(Main.spriteBatch);
        
        string statusText = Main.statusText;
        GenerationProgress progress = WorldGenerator.CurrentGenerationProgress;
        
        
        const int TipOffset = 438;
        DrawStringCentered(Language.GetTextValue("Mods.WaterMod.SubworldEnterText.SeamapEnter"), Color.White, new Vector2(0, TipOffset - 38), 0.8f);
        
        
        if (SubworldSystem.Current is not null)
        {
            DrawStringCentered(Language.GetTextValue("Mods.WaterMod.Entering"), Color.LightGray, new Vector2(0, -360), 0.4f, true);
            DrawStringCentered(this.Name, Color.White, new Vector2(0, -310), 1.1f, true);
        }
    }
    
    private static void DrawStringCentered(string test, Color color, Vector2 position = default, float scale = 1f, bool outlined = false)
    {
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + position;
        DynamicSpriteFont font = FontAssets.DeathText.Value;
        Vector2 halfSize = font.MeasureString(test) / 2f * scale;

        if (!outlined)
        {
            Main.spriteBatch.DrawString(font, test, screenCenter - halfSize, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        else
        {
            float off = 6 * MathHelper.Lerp(scale, 1, 0.3f);
            Color shadowColor = Color.Black;
            Color textColor = Color.White;
            Vector2 drawPos = screenCenter - halfSize;

            ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(off, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
            ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(0, off), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
            ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(off, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
            ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(0, off), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
            ChatManager.DrawColorCodedString(Main.spriteBatch, font, test, drawPos, textColor, 0f, Vector2.Zero, new Vector2(scale));
        }
    }
}