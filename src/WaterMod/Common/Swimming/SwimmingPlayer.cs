using System;
using Terraria.DataStructures;
using Terraria.WorldBuilding;
using WaterMod.Common.World;
using WaterMod.Utilities;

namespace WaterMod.Common.Swimming;

internal sealed class SwimmingPlayer : ModPlayer {
    private StatModifier speedModifier = new();
    private StatModifier accelerationModifier = new();

    // TODO: Move this to a separate player to provide usability across the entire project, and not just this.
    private bool oldUnderwater;

    // TODO: Ensure this works properly across multiplayer.
    private Vector2 velocity;

    private float bodyRotation;
    private float headRotation;

    private float targetBodyRotation;
    private float targetHeadRotation;

    public override void PostUpdate() {
        base.PostUpdate();

        UpdateMovement();
        UpdateVisuals();
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
        base.ModifyDrawInfo(ref drawInfo);

        if (Main.gameMenu) {
            return;
        }
        
        if (Player.IsMounted()) {
            return;
        }

        var drawPlayer = drawInfo.drawPlayer;

        drawPlayer.headRotation = headRotation;
        drawPlayer.fullRotation = bodyRotation;
        drawPlayer.fullRotationOrigin = drawPlayer.Size / 2f;

        if (!Player.IsUnderwater() || (!Player.HeldItem.IsAir && Player.controlUseItem)) {
            return;
        }

        var swimSpeedFactor = Player.velocity.Length() * 0.25f;
        var swimArmRotation = MathF.Sin(Main.GameUpdateCount * 0.1f) * swimSpeedFactor;

        if (Player.direction == 1) {
            swimArmRotation += MathHelper.Pi;
        }

        drawPlayer.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, bodyRotation + swimArmRotation - MathHelper.PiOver2);
        drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, bodyRotation - swimArmRotation - MathHelper.PiOver2);
    }

    public ref StatModifier GetMovementSpeed() {
        return ref speedModifier;
    }

    public ref StatModifier GetMovementAcceleration() {
        return ref accelerationModifier;
    }

    private void UpdateMovement() {
        if (Player.IsUnderwater() && !oldUnderwater) {
            velocity = Player.velocity;
        }

        if (!Player.IsUnderwater()) {
            velocity = Vector2.Zero;
        }
        else {
            var direction = new Vector2(
                Player.controlRight.ToInt() + -Player.controlLeft.ToInt(),
                Player.controlDown.ToInt() + -Player.controlUp.ToInt()
            );

            direction = direction.SafeNormalize(Vector2.Zero);

            if (direction.LengthSquared() > 0f) {
                var acceleration = accelerationModifier.ApplyTo(0.25f);
                var speed = speedModifier.ApplyTo(4f);

                velocity += direction * acceleration;
                velocity = Vector2.Clamp(velocity, new Vector2(-speed), new Vector2(speed));
            }
            else {
                velocity *= 0.95f;
            }

            Player.velocity = velocity;
        }

        oldUnderwater = Player.IsUnderwater();
    }

    private void UpdateVisuals() {
        // TODO: Maybe make this an extension for accessibility across the project?
        var diving = Player.velocity.Y > 0f && !Player.IsMounted() && !Player.IsUnderwater() && WorldUtils.Find
        (
            Player.Center.ToTileCoordinates() - new Point(1, 0),
            Searches.Chain
            (
                new Searches.Rectangle(2, 2),
                new HasWaterCondition(),
                new HasTileCondition().Not(),
                new Conditions.IsSolid().Not()
            ),
            out var _
        );

        if (Player.IsUnderwater() || diving) {
            var rotation = Player.velocity.ToRotation();

            if (Player.direction == -1) {
                rotation = MathHelper.WrapAngle(rotation + MathHelper.Pi);
            }

            var maxHeadRotation = MathHelper.ToRadians(80f);
            var minHeadRotation = MathHelper.ToRadians(-80f);

            targetHeadRotation = MathHelper.Clamp(rotation, minHeadRotation, maxHeadRotation);

            if (Player.direction == -1) {
                targetHeadRotation += MathHelper.PiOver4;
            }
            else {
                targetHeadRotation -= MathHelper.PiOver4;
            }

            targetBodyRotation = Player.velocity.ToRotation() + MathHelper.PiOver2;

            if (Player.velocity.LengthSquared() > 0f) {
                targetBodyRotation = Player.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else {
                targetBodyRotation = 0f;
            }
        }
        else {
            targetHeadRotation = 0f;
            targetBodyRotation = 0f;
        }

        headRotation = headRotation.AngleLerp(targetHeadRotation, 0.2f);
        bodyRotation = bodyRotation.AngleLerp(targetBodyRotation, 0.2f);
    }
}