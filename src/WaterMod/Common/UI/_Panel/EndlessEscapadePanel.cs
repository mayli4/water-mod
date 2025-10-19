using Daybreak.Common.Features.ModPanel;
using Daybreak.Common.Rendering;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace WaterMod.Common.UI._Panel;

internal sealed class EndlessEscapadePanel : ModPanelStyleExt {
    public override UIImage? ModifyModIcon(UIModItem element, UIImage modIcon, ref int modIconAdjust) => null;

    public override bool PreDrawPanel(UIModItem element, SpriteBatch sb, ref bool drawDivider) {
        drawDivider = false;
        var dims = element.GetDimensions();
        
        if (element._needsTextureLoading) {
            element._needsTextureLoading = false;
            element.LoadTextures();
        }

        var effect = Shaders.Panel.WavingWater.Value;
        
        sb.End(out var ss);
        sb.Begin(new SpriteBatchSnapshot() with { TransformMatrix = Main.UIScaleMatrix, CustomEffect = effect });
        
        element.DrawPanel(sb, element._borderTexture.Value, element.BorderColor);
        sb.Restart(in ss);
        return false;
    }
    
    public override Color ModifyEnabledTextColor(bool enabled, Color color) => enabled ? Color.Aquamarine : Color.MediumAquamarine;
}