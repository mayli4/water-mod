using WaterMod.Generator.Utils;

namespace WaterMod.Generator.Models;

internal record struct PacketModel(string? Namespace, TypeDeclarationModel Type, EquatableArray<TypeDeclarationModel> NestedTypes);