using Stride.Core;
using Stride.Core.Reflection;
namespace R3.Stride;
internal class Module
{
    [ModuleInitializer]
    public static void Initialize()
    {
        AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
    }
}
