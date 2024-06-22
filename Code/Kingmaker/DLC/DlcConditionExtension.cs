using Kingmaker.Blueprints;

namespace Kingmaker.DLC;

public static class DlcConditionExtension
{
	public static bool IsDlcRestricted(this BlueprintScriptableObject blueprint)
	{
		if (blueprint != null)
		{
			return !(blueprint.GetComponent<DlcCondition>()?.IsFullFilled() ?? true);
		}
		return false;
	}
}
