using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace R3.Generator;

public record ParseResult(
    TypeDeclarationSyntax TargetTypeSyntax,
    INamedTypeSymbol TargetTypeSymbol,
    bool IsStatic,
    ObservableTriggerAttribute[] Attributes
);

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ObservableTriggerAttribute : Attribute
{
    public TriggerKinds Kinds { get; }
    public bool DefineCoreMethod { get; }

    public ObservableTriggerAttribute(TriggerKinds kinds, bool defineCoreMethod = false)
    {
        this.Kinds = kinds;
        this.DefineCoreMethod = defineCoreMethod;
    }
}

public class ObservableTriggerParser(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> sources)
{
    // [ObservableTrigger]
    // public class | public static class
    public ParseResult[] Parse()
    {
        var list = new List<ParseResult>();
        foreach (var source in sources)
        {
            var targetType = (TypeDeclarationSyntax)source.TargetNode;
            var symbol = (INamedTypeSymbol)source.TargetSymbol;
            if (symbol == null) continue;

            // verify is partial
            if (!IsPartial(targetType))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, targetType.Identifier.GetLocation(), symbol.Name));
                continue;
            }

            // nested is not allowed
            if (IsNested(targetType))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NestedNotAllow, targetType.Identifier.GetLocation(), symbol.Name));
                continue;
            }

            // verify is generis type
            if (symbol.TypeParameters.Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GenericTypeNotSupported, targetType.Identifier.GetLocation(), symbol.Name));
                continue;
            }

            var attr = GetAttribute(source);

            var result = new ParseResult(targetType, symbol, symbol.IsStatic, attr);
            list.Add(result);
        }

        return list.ToArray();
    }

    static bool IsPartial(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    static bool IsNested(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Parent is TypeDeclarationSyntax;
    }

    static ObservableTriggerAttribute[] GetAttribute(GeneratorAttributeSyntaxContext source)
    {
        var attributes = new ObservableTriggerAttribute[source.Attributes.Length];
        var i = 0;
        foreach (var attributeData in source.Attributes)
        {
            var ctorItems = attributeData.ConstructorArguments;

            var triggerKinds = (TriggerKinds)ctorItems[0].Value!;
            var defineCore = (bool?)ctorItems[1].Value ?? false;

            attributes[i++] = new ObservableTriggerAttribute(triggerKinds, defineCore);
        }

        return attributes;
    }
}
