using Generators.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Generators;

[Generator(LanguageNames.CSharp)]
public class ModImplGenerator : IIncrementalGenerator {
    void IIncrementalGenerator.Initialize(
        IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (x, options) => {
                if(GeneratorUtils.GetRootNamespaceOrRaiseDiagnostic(x, options.GlobalOptions) is not { } rootNamespace)
                    return;

                x.AddSource(
                    "_ModImpl.g.cs",
                    SourceText.From(GenerateUsings(rootNamespace), Encoding.UTF8)
                );
            }
        );

        string GenerateUsings(string rootNamespace) {
            var sb = new StringBuilder();
            sb.Append("using Terraria.ModLoader;\nnamespace WaterMod;\npublic partial class WaterModImpl : Mod;");
            return sb.ToString();
        }
    }
}