using System.Runtime.CompilerServices;
using Terraria.DataStructures;

namespace WaterMod.Utilities;

internal static class PlayerExtensions {
    extension(PlayerDrawSet drawInfo) {
        public Vector2 HeadPosition() => drawInfo.drawPlayer.GetHelmetDrawOffset() + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect + drawInfo.helmetOffset;
        public Vector2 BodyPosition() => new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        public Vector2 LegsPosition() => new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect;
    }
    
    extension(PlayerDrawSet drawInfo)
    {
        public Vector2 HeadPosition(bool addBob = false, bool vanillaStyle = false) {
            //drawInfo.drawPlayer.GetHelmetDrawOffset() + 
            var drawPosition = GetFrameOrigin(drawInfo);

            if (vanillaStyle)
                drawPosition += drawInfo.drawPlayer.headPosition + drawInfo.headVect;
            else {
                if (drawInfo.drawPlayer.gravDir == -1)
                    drawPosition.Y = (int)drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.bodyFrame.Height - 4f;

                var headOffset = drawInfo.drawPlayer.headPosition + drawInfo.headVect;

                if (!drawInfo.drawPlayer.dead && drawInfo.drawPlayer.gravDir == -1)
                    headOffset.Y -= 6;

                headOffset.Y *= drawInfo.drawPlayer.gravDir;
                drawPosition += headOffset;
            }

            if (addBob)
                drawPosition += Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height] * drawInfo.drawPlayer.gravDir;

            return drawPosition;
        }

        public Vector2 GetFrameOrigin() {
            return new(
                (int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.bodyFrame.Width / 2) + drawInfo.drawPlayer.width / 2),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f));
        }
    }

    extension(Player player)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnderwater(bool includeSlopes = false) {
            return Collision.DrownCollision(player.position, player.width, player.height, player.gravDir, includeSlopes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGrounded() {
            return player.velocity.Y == 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMounted() {
            return player.mount.Active;
        }
    }
}