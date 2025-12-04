using WaterMod.Generator.Utils;

namespace WaterMod.Generator.Models;

internal record struct PacketModel(string? Namespace, string FullyQualifiedName, TypeDeclarationModel Type, EquatableArray<TypeDeclarationModel> NestedTypes);