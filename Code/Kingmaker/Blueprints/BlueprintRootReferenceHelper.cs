using Kingmaker.Blueprints.Root;

namespace Kingmaker.Blueprints;

public class BlueprintRootReferenceHelper
{
	public static BlueprintReference<BlueprintRoot> RootRef { get; } = new BlueprintRootReference();


	public static BlueprintRoot GetRoot()
	{
		return RootRef.Get();
	}
}
