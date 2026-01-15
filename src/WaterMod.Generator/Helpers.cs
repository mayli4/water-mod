using Microsoft.CodeAnalysis;

namespace WaterMod.Generator;

internal static class Helpers
{
    public static bool IsPacketHandlerAttribute(this INamedTypeSymbol? namedTypeSymbol) => namedTypeSymbol is
    {
        Name: "PacketHandlerAttribute",
        ContainingNamespace:
        {
            Name: "Generator",
            ContainingNamespace:
            {
                Name: "WaterMod",
                ContainingNamespace
                    .IsGlobalNamespace: true
            }
        }
    };
}