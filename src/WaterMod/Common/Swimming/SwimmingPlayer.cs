using System;
using Terraria.DataStructures;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.WorldBuilding;
using WaterMod.Common.World;
using WaterMod.Utilities;

namespace WaterMod.Common.Swimming;

internal sealed class SwimmingPlayer : ModPlayer {
    private StatModifier _speedModifier = new();
    private StatModifier _accelerationModifier = new();

    // TODO: Move this to a separate player to provide usability across the entire project, and not just this.
    private bool _oldUnderwater;

    // TODO: Ensure this works properly across multiplayer.
    private Vector2 _velocity;

    private float _bodyRotation;
    private float _headRotation;

    private float _targetBodyRotation;
    private float _targetHeadRotation;
    
    public override void ResetEffects() {
        base.ResetEffects();

        _speedModifier = new();
        _accelerationModifier = new();
    }

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

        drawPlayer.headRotation = _headRotation;
        drawPlayer.fullRotation = _bodyRotation;
        drawPlayer.fullRotationOrigin = drawPlayer.Size / 2f;

        if (!Player.IsUnderwater() || (!Player.HeldItem.IsAir && Player.controlUseItem)) {
            return;
        }

        var swimSpeedFactor = Player.velocity.Length() * 0.25f;
        var swimArmRotation = MathF.Sin(Main.GameUpdateCount * 0.1f) * swimSpeedFactor;

        if (Player.direction == 1) {
            swimArmRotation += MathHelper.Pi;
        }

        drawPlayer.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, _bodyRotation + swimArmRotation - MathHelper.PiOver2);
        drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _bodyRotation - swimArmRotation - MathHelper.PiOver2);
    }

    public ref StatModifier GetMovementSpeed() {
        return ref _speedModifier;
    }

    public ref StatModifier GetMovementAcceleration() {
        return ref _accelerationModifier;
    }

    private void UpdateMovement() {
        if (Player.IsUnderwater() && !_oldUnderwater) {
            _velocity = Player.velocity;
        }

        if (!Player.IsUnderwater()) {
            _velocity = Vector2.Zero;
        }
        else {
            var direction = new Vector2(
                Player.controlRight.ToInt() + -Player.controlLeft.ToInt(),
                Player.controlDown.ToInt() + -Player.controlUp.ToInt()
            );

            if (Player.mount.Active) Player.mount.Dismount(Player);

            direction = direction.SafeNormalize(Vector2.Zero);

            if (direction.LengthSquared() > 0f) {
                var acceleration = _accelerationModifier.ApplyTo(0.25f);
                var speed = _speedModifier.ApplyTo(4f);

                _velocity += direction * acceleration;
                _velocity = Vector2.Clamp(_velocity, new Vector2(-speed), new Vector2(speed));
                
                if (Filters.Scene["WaterDistortion"].GetShader() is not WaterShaderData data)
                    return;
                
                data.QueueRipple(Player.Center, 5, RippleShape.Circle, MathHelper.PiOver4);
            }
            else {
                _velocity *= 0.95f;
            }

            Player.velocity = _velocity;
        }

        _oldUnderwater = Player.IsUnderwater();
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

            _targetHeadRotation = MathHelper.Clamp(rotation, minHeadRotation, maxHeadRotation);

            if (Player.direction == -1) {
                _targetHeadRotation += MathHelper.PiOver4;
            }
            else {
                _targetHeadRotation -= MathHelper.PiOver4;
            }

            _targetBodyRotation = Player.velocity.ToRotation() + MathHelper.PiOver2;

            if (Player.velocity.LengthSquared() > 0f) {
                _targetBodyRotation = Player.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else {
                _targetBodyRotation = 0f;
            }
        }
        else {
            _targetHeadRotation = 0f;
            _targetBodyRotation = 0f;
        }

        _headRotation = _headRotation.AngleLerp(_targetHeadRotation, 0.2f);
        _bodyRotation = _bodyRotation.AngleLerp(_targetBodyRotation, 0.2f);
    }
}