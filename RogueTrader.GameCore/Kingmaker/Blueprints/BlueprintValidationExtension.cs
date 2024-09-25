using System.Linq;

namespace Kingmaker.Blueprints;

public static class BlueprintValidationExtension
{
	public static bool HasLogic<TLogic>(this BlueprintScriptableObject blueprint) where TLogic : class
	{
		return blueprint.ComponentsArray.OfType<TLogic>().Any();
	}

	public static bool HasExactlyOneLogic<TLogic>(this BlueprintScriptableObject blueprint) where TLogic : class
	{
		return blueprint.ComponentsArray.OfType<TLogic>().Count() == 1;
	}

	public static bool HasAtMostOneLogic<TLogic>(this BlueprintScriptableObject blueprint) where TLogic : class
	{
		return blueprint.ComponentsArray.OfType<TLogic>().Count() <= 1;
	}
}
