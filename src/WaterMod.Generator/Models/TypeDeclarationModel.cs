using Microsoft.CodeAnalysis;

namespace WaterMod.Generator.Models;

internal record struct TypeDeclarationModel(bool IsRecord, TypeKind TypeKind, string Name);