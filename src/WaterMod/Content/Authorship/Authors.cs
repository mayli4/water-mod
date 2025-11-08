using Daybreak.Common.Features.Authorship;

namespace WaterMod.Content.Authorship;

public abstract class CommonAuthorTag : AuthorTag {
    private const string suffix = "Tag";
    public override string Name => base.Name.EndsWith(suffix) ? base.Name[..^suffix.Length] : base.Name;
    public override string Texture => string.Join('/', Assets.Textures.UI.Mathica.KEY.Split('/')[..^1]) + '/' + Name;
}

public class MathicaTag : CommonAuthorTag;