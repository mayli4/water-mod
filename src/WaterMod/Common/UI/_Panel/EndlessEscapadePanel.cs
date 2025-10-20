using Daybreak.Common.Features.ModPanel;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace WaterMod.Common.UI;


#nullable enable
internal sealed class EndlessEscapadePanel : ModPanelStyleExt {
    public override UIImage? ModifyModIcon(UIModItem element, UIImage modIcon, ref int modIconAdjust) => null;

    public override Color ModifyEnabledTextColor(bool enabled, Color color) =>
        enabled ? Color.Aquamarine : Color.MediumAquamarine;

    public override bool PreDrawPanel(UIModItem element, SpriteBatch sb, ref bool drawDivider) {
        drawDivider = false;
        var dims = element.GetDimensions();

        if(element._needsTextureLoading) {
            element._needsTextureLoading = false;
            element.LoadTextures();
        }

        var effect = Shaders.Panel.WavingWater.Value;
        
        float uDaylightIntensityValue = 0.0f;
        float dawnDuskTransitionDuration = 3600f; 

        if (Main.dayTime) {
            float currentTime = (float)Main.time;
            float dayLength = (float)Main.dayLength - 2000;
            
            float transitionRatio = dawnDuskTransitionDuration / dayLength;

            float dayProgress = currentTime / dayLength;

            float dawnRamp = MathHelper.Clamp(dayProgress / transitionRatio, 0.0f, 1.0f);
            float duskRamp = MathHelper.Clamp((1.0f - dayProgress) / transitionRatio, 0.0f, 1.0f);
            uDaylightIntensityValue = Math.Min(dawnRamp, duskRamp);
        }
        
        effect.Parameters["source"].SetValue(Transform(new Vector4(dims.Width, dims.Height, dims.X, dims.Y)));
        effect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
        effect.Parameters["water_effect_power"].SetValue(1.0f);
        effect.Parameters["wave_amplitude"].SetValue(0.06f);
        effect.Parameters["wave_frequency"].SetValue(2.0f);
        effect.Parameters["wave_speed"].SetValue(2.0f);
        effect.Parameters["water_base_level"].SetValue(0.3f);
        effect.Parameters["pixel"].SetValue(2f);
        
        effect.Parameters["water_gradient_intensity"].SetValue(1.5f);
        
        effect.Parameters["cloud_color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.8f));
        effect.Parameters["cloud_density"].SetValue(0.9f);
        effect.Parameters["cloud_scale"].SetValue(0.3f);
        effect.Parameters["cloud_speed"].SetValue(0.05f);
        
        effect.Parameters["uTexture"].SetValue(Textures.Sample.Pebbles.Value); 
        effect.Parameters["uWaterNoiseTexture"].SetValue(Textures.Sample.Pebbles.Value); 
        
        effect.Parameters["daylight_intensity"].SetValue(uDaylightIntensityValue);
        
        //day colors
        effect.Parameters["sky_gradient_top_color_day"].SetValue(new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
        effect.Parameters["sky_gradient_bottom_color_day"].SetValue(new Vector4(0.5f, 0.7f, 1.0f, 1.0f));
        
        effect.Parameters["water_top_color_day"].SetValue(new Color(37, 86, 132).ToVector4());
        effect.Parameters["water_bottom_color_day"].SetValue(new Color(25, 58, 87).ToVector4());
        
        effect.Parameters["surface_line_color_day"].SetValue(new Color(137, 186, 232).ToVector4());

        //night colors
        effect.Parameters["sky_gradient_top_color_night"].SetValue(new Vector4(0.02f, 0.05f, 0.15f, 1.0f));
        effect.Parameters["sky_gradient_bottom_color_night"].SetValue(new Color(148, 88, 97).ToVector4());
        
        effect.Parameters["water_top_color_night"].SetValue(new Vector4(0.0f, 0.05f, 0.15f, 1.0f));
        effect.Parameters["water_bottom_color_night"].SetValue(new Color(43, 37, 28).ToVector4());
        
        effect.Parameters["surface_line_color_night"].SetValue(new Vector3(0.2f, 0.3f, 0.4f));
        
        effect.Parameters["water_scroll_speed"].SetValue(-0.3f);
        effect.Parameters["color_quantization_resolution"].SetValue(new Vector4(24.0f, 24.0f, 24.0f, 24.0f));
        effect.Parameters["star_intensity"].SetValue(0.5f);
        effect.Parameters["atmosphere_edge_color"].SetValue(new Vector4(0.05f, 0.08f, 0.15f, 0.3f));
        effect.Parameters["atmosphere_curve_strength"].SetValue(0.7f);
        
        
        sb.End(out var ss);
        sb.Begin(new SpriteBatchSnapshot() with { TransformMatrix = Main.UIScaleMatrix, CustomEffect = effect });

        element.DrawPanel(sb, element._backgroundTexture.Value, element.BorderColor);
        sb.Restart(in ss);
        
        element.DrawPanel(sb, element._borderTexture.Value, element.BorderColor);
        return false;
    }
    
    private static Vector4 Transform(Vector4 vector) {
        var vec1 = Vector2.Transform(new Vector2(vector.X, vector.Y), Main.UIScaleMatrix);
        var vec2 = Vector2.Transform(new Vector2(vector.Z, vector.W), Main.UIScaleMatrix);
        return new Vector4(vec1, vec2.X, vec2.Y);
    }

    internal sealed class BoatIcon() {
        
    }
}