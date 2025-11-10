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
    public int ShipMaxLife { get; set; } = 100;
    public int ShipCurrentLife { get; set; } = 100;
    private int _previousShipCurrentLife;

    public override void Initialize() {
        _previousShipCurrentLife = ShipCurrentLife;
    }

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
            if (Player.whoAmI == Main.myPlayer || Main.netMode == NetmodeID.Server) {
                Player.KillMe(PlayerDeathReason.ByOther(1), 1, 0);
            }
        }
    }
    
    public void HealShipDamage(int amount) {
        ShipCurrentLife += amount;
        if (ShipCurrentLife > ShipMaxLife) {
            ShipCurrentLife = ShipMaxLife;
        }
    }

    public override void PostUpdate() {
        if(Main.keyState.IsKeyDown(Keys.O) && !Main.oldKeyState.IsKeyDown(Keys.O) ) {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI == Main.myPlayer) {
                TakeShipDamage(5);
            }
        }
        
        if(Main.keyState.IsKeyDown(Keys.P) && !Main.oldKeyState.IsKeyDown(Keys.P) ) {
            if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI == Main.myPlayer) {
                HealShipDamage(5);
            }
        }
        
        if (ShipCurrentLife != _previousShipCurrentLife) {
            _previousShipCurrentLife = ShipCurrentLife;
            
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) {
                new ShipHealthPacket(Player.whoAmI, ShipCurrentLife).Send();
            } else if (Main.netMode == NetmodeID.Server) {
                new ShipHealthPacket(Player.whoAmI, ShipCurrentLife).Send(ignoreClient: Player.whoAmI);
            }
        }
    }

#if  DEBUG
    [UsedImplicitly]
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
#endif
}


[Packet]
internal partial record struct ShipHealthPacket(int PlayerId, int CurrentShipHealth) {
    [PacketHandler]
    public static void OnReceive(in ShipHealthPacket packet, int whoAmI) {
        packet.Deconstruct(out int playerId, out int currentShipHealth);

        if (playerId is < 0 or >= Main.maxPlayers)
            return;

        var player = Main.player[playerId];
        if (player.active) {
            SeamapPlayer seamapPlayer = player.GetModPlayer<SeamapPlayer>();
            seamapPlayer.ShipCurrentLife = currentShipHealth;
        }

        if (Main.netMode == NetmodeID.Server) {
            new ShipHealthPacket(playerId, currentShipHealth).Send(ignoreClient: whoAmI);
        }
    }
}