using System;
using Daybreak.Common.Rendering;
using Terraria.Graphics.Renderers;
using WaterMod.Common.Rendering;

namespace WaterMod.Content.Particles;

public class GenericSmallSparkle : BaseParticle<GenericSmallSparkle> {
    public Color ColorTint;
    public int LifeTime;
    private int maxLifeTime;

    public Vector2 Position;
    public float Rotation;
    public float Scale;
    public Vector2 Velocity;

    public float Spin;
    public float HueShift;
    
    private float opacity;

    public static GenericSmallSparkle RequestNew(
        Vector2 position,
        Vector2 velocity,
        Color color,
        float scale,
        int lifeTime,
        float rotationSpeed = 0.15f,
        float hueShift = 0f
    ) {
        var particle = Pool.RequestParticle();
        particle.Position = position;
        particle.Velocity = velocity;
        particle.ColorTint = color;
        particle.Scale = scale;
        particle.maxLifeTime = lifeTime;
        particle.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        particle.Spin = rotationSpeed;
        particle.HueShift = hueShift;
        return particle;
    }

    public override void FetchFromPool() {
        base.FetchFromPool();
        LifeTime = 0;
        opacity = 0f;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        var completion = (float)LifeTime / maxLifeTime;
        opacity = (float)Math.Sin(MathHelper.PiOver2 + completion * MathHelper.PiOver2);

        Position += Velocity;
        Velocity *= 0.80f;

        Rotation += Spin * (Velocity.X > 0 ? 1f : -1f) * (completion > 0.5f ? 1f : 0.5f);

        var hsl = Main.rgbToHsl(ColorTint);
        ColorTint = Main.hslToRgb((hsl.X + HueShift) % 1, hsl.Y, hsl.Z);

        Lighting.AddLight(Position, ColorTint.ToVector3() * opacity);

        if (++LifeTime >= maxLifeTime) {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(
        ref ParticleRendererSettings settings,
        SpriteBatch spritebatch
    ) {
        var texture = Assets.Images.Particles.ThinSparkle.Asset.Value;
        
        
        spritebatch.End(out var ss);
        spritebatch.Begin(ss with { BlendState = BlendState.Additive });
        spritebatch.Draw(
            texture,
            Position + settings.AnchorPosition,
            null,
            ColorTint * opacity,
            Rotation,
            texture.Size() / 2,
            Scale,
            SpriteEffects.None,
            0
        );
        
        spritebatch.Restart(ss);
    }
}