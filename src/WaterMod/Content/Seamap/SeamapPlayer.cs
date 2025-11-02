using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WaterMod.Content.Seamap;

internal sealed class SeamapPlayer : ModPlayer {
    //todo make this more robust, upgrades etc
    public int ShipMaxLife { get; set; } = 100;
    public int ShipCurrentLife { get; set; } = 100;
    
    public override void PreUpdate() {
        if (!SubworldSystem.IsActive<SeamapSubworld>())
            return;

        Player.position = Player.oldPosition;

        Player.position.X = 0;
        Player.position.Y = 0;

        Player.fallStart = (int)(Player.position.Y / 16f);
    }

    public void TakeShipDamage(int damage) {
        ShipCurrentLife -= damage;
        if (ShipCurrentLife < 0) {
            ShipCurrentLife = 0;
            Player.KillMe(PlayerDeathReason.ByOther(1), 1, 0);
        }
    }

    public override void PostUpdate() {
        if(Main.keyState.IsKeyDown(Keys.O) && !Main.oldKeyState.IsKeyDown(Keys.O) ) {
            TakeShipDamage(5);
        }
        
        //Main.NewText("ship life: " + ShipCurrentLife);
    }

    public override void SendClientChanges(ModPlayer clientPlayer) {
        base.SendClientChanges(clientPlayer);
    }

    public override void CopyClientState(ModPlayer targetCopy) {
        base.CopyClientState(targetCopy);
    }

    public override void SaveData(TagCompound tag) {
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag) {
        base.LoadData(tag);
    }

    [SubscribesTo<ModSystemHooks.PostDrawTiles>]
    static void DrawLifeText(ModSystemHooks.PostDrawTiles.Original orig, ModSystem system) {
        orig();
        
        Main.spriteBatch.Begin();
        
        for (int i = 0; i < Main.maxPlayers; i++) {
            var currentPlayer = Main.player[i];
            if (currentPlayer.active && !currentPlayer.dead) {
                var player = currentPlayer.GetModPlayer<SeamapPlayer>();

                string healthText = $"ship hp: {player.ShipCurrentLife}/{player.ShipMaxLife}";
                Color textColor = Color.White;
                                    
                var font = FontAssets.MouseText.Value;

                var textSize = font.MeasureString(healthText);

                var drawPos = currentPlayer.Center - Main.screenPosition;
                drawPos.Y += currentPlayer.height / 2 + 10;
                drawPos.X -= textSize.X / 2;

                Main.spriteBatch.DrawString(font, healthText, drawPos, textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
        
        Main.spriteBatch.End();
    }
}