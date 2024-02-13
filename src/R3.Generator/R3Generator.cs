using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace R3.Generator;

[Generator(LanguageNames.CSharp)]
public partial class R3Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "R3.ObservableTriggerAttribute",
            static (node, token) => node is ClassDeclarationSyntax,
            static (context, token) => context);

        context.RegisterSourceOutput(source.Collect(), ExecuteForObservableTrigger);
    }

    static void ExecuteForObservableTrigger(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> sources)
    {
        if (sources.Length == 0) return;

        var result = new ObservableTriggerParser(context, sources).Parse();
        if (result.Length != 0)
        {
            var emitter = new ObservableTriggerEmitter(context, result);
            emitter.Emit();
        }
    }
}
