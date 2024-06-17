using Kingmaker.Blueprints;

namespace Code.GameCore.Blueprints.Workarounds;

public static class BlueprintRootInstanceHelper
{
	public static T GetInstance<T>() where T : SimpleBlueprint
	{
		return null;
	}
}
