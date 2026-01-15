using Daybreak.Common.Features.Hooks;
using Terraria.Graphics.Renderers;

namespace WaterMod.Common.Rendering;

/// <summary>
///     Contains particle systems.
/// </summary>
public static class ParticleEngine
{
    /// <summary>
    ///     Renders behind dust.
    /// </summary>
    public static readonly ParticleRenderer PARTICLES = new();

    /// <summary>
    ///     Renders behind front gore.
    /// </summary>
    public static readonly ParticleRenderer GORE_LAYER = new();

    [OnLoad]
    public static void Load()
    {
        On_Main.DrawDust += DrawParticles;
        On_Main.DrawGore += DrawGoreParticles;
        On_Main.UpdateParticleSystems += UpdateParticles;
    }

    private static void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
    {
        orig(self);

        PARTICLES.Update();
        GORE_LAYER.Update();
    }

    private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        PARTICLES.Settings.AnchorPosition = -Main.screenPosition;
        PARTICLES.Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        orig(self);
    }

    private static void DrawGoreParticles(On_Main.orig_DrawGore orig, Main self)
    {
        //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        GORE_LAYER.Settings.AnchorPosition = -Main.screenPosition;
        GORE_LAYER.Draw(Main.spriteBatch);
        //Main.spriteBatch.End();

        orig(self);
    }
}