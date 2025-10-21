using Daybreak.Common.Features.ModPanel;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace WaterMod.Common.UI;

#nullable enable
internal sealed class EndlessEscapadePanel : ModPanelStyleExt {
    public static float CurrentDaylightIntensity;
    
    private static RenderTarget2D? PanelTarget;
    
    public override Color ModifyEnabledTextColor(bool enabled, Color color) =>
        enabled ? Color.Aquamarine : Color.MediumAquamarine;
    
    public override UIText ModifyModName(UIModItem element, UIText modName) {
        return new ModName("Endless Escapade" + $" v{element._mod.Version}")
        {
            Left = modName.Left,
            Top = modName.Top,
        };
    }
    
    public override UIImage ModifyModIcon(UIModItem element, UIImage modIcon, ref int modIconAdjust) {
        return new BoatIcon() {
            Left = modIcon.Left,
            Top = modIcon.Top,
            Width = modIcon.Width,
            Height = modIcon.Height,
        };
    }
    
    public override Dictionary<TextureKind, Asset<Texture2D>> TextureOverrides { get; } = new() {
        { TextureKind.ModInfo, Textures.UI.InfoIcon },
        { TextureKind.ModConfig, Textures.UI.ConfigIcon },
        { TextureKind.Deps, Textures.UI.DepsIcon }
    };

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
        
        EndlessEscapadePanel.CurrentDaylightIntensity = uDaylightIntensityValue;
        
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
        // effect.Parameters["cloud_color_day"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.8f));
        // effect.Parameters["cloud_color_night"].SetValue(new Vector4(0.5f, 0.5f, 0.5f, 10f));
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
    internal sealed class BoatIcon : UIImage {
        private readonly Asset<Texture2D> iconAsset;
        
        public BoatIcon() : base(TextureAssets.MagicPixel) {
            iconAsset = Textures.UI.ModIcon_Flag;
            
            Width.Set(96, 0f);
            Height.Set(80, 0f);
        }

        public override void DrawSelf(SpriteBatch spriteBatch) {
            int currentFrame = (int)((Main.GlobalTimeWrappedHourly / 0.25f) % 3);

            var sourceRect = new Rectangle(
                currentFrame * 96,
                0,
                96,
                80
            );

            var dims = GetDimensions();
            
            Vector2 origin = new Vector2(-96, 80 / 2);

            var offsetpos = new Vector2(-98, 40); 
            var offsetposPole = new Vector2(-90, 36); 
            
            float swayRotation = MathF.Sin(Main.GlobalTimeWrappedHourly * 1.5f) * 0.01f;
            
            spriteBatch.Draw(Textures.UI.ModIcon_Pole.Value, dims.Position() + offsetposPole, null, Color.White, 0.0f, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(iconAsset.Value, dims.Position() + offsetpos, sourceRect, Color.White, swayRotation, origin, 1f, SpriteEffects.None, 0f);
        }
    }
    
    public sealed class ModName : UIText {
        private readonly string originalText;

        private static readonly Color DayTextPulseStart = Color.LightYellow;
        private static readonly Color DayTextPulseEnd = Color.Orange;
        private static readonly Color NightTextPulseStart = Color.SkyBlue;
        private static readonly Color NightTextPulseEnd = Color.MediumPurple;


        public ModName(string text, float textScale = 1, bool large = false) : base(text, textScale, large) {
            if (ChatManager.Regexes.Format.Matches(text).Count != 0) {
                throw new InvalidOperationException("The text cannot contain formatting.");
            }

            originalText = text;
        }

        public override void DrawSelf(SpriteBatch spriteBatch) {
            var formattedText = GetPulsatingText(originalText, Main.GlobalTimeWrappedHourly, EndlessEscapadePanel.CurrentDaylightIntensity);
            SetText(formattedText);

            base.DrawSelf(spriteBatch);
        }

        public static string GetPulsatingText(string text, float time, float daylightIntensity) {
            var currentLightColor = Color.Lerp(NightTextPulseStart, DayTextPulseStart, daylightIntensity);
            var currentDarkColor = Color.Lerp(NightTextPulseEnd, DayTextPulseEnd, daylightIntensity);

            const float speed = 3f;
            const float offset = 0.3f;

            // [c/______:x]
            const int character_length = 12;

            var sb = new StringBuilder(character_length * text.Length);
            for (var i = 0; i < text.Length; i++) {
                var wave = MathF.Sin(time * speed + i * offset);
                var color = Color.Lerp(currentLightColor, currentDarkColor, (wave + 1f) / 2f);

                sb.Append($"[c/{color.Hex3()}:{text[i]}]");
            }

            return sb.ToString();
        }
    }
}