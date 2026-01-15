using Daybreak.Common.Features.Authorship;
using JetBrains.Annotations;

namespace WaterMod.Content;

public abstract class CommonAuthorTag : AuthorTag {
    private const string suffix = "Tag";
    public override string Name => base.Name.EndsWith(suffix) ? base.Name[..^suffix.Length] : base.Name;
    public override string Texture => string.Join('/', Assets.Textures.UI.Mathica.KEY.Split('/')[..^1]) + '/' + Name;
}

[UsedImplicitly] internal class MathicaTag : CommonAuthorTag;
[UsedImplicitly] internal class RotonTag : CommonAuthorTag;
[UsedImplicitly] internal class PaperclipTag : CommonAuthorTag;
[UsedImplicitly] internal class JaqbixTag : CommonAuthorTag;
[UsedImplicitly] internal class FunkDotItTag : CommonAuthorTag;
[UsedImplicitly] internal class TyeskiTag : CommonAuthorTag;