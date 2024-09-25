using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("99fff743eb734f59b275ee3142de9883")]
public class ContextConditionZoneCR : ContextCondition
{
	public int MinimalCR;

	protected override string GetConditionCaption()
	{
		return "Is are CR higher or equal";
	}

	protected override bool CheckCondition()
	{
		return (Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0) >= MinimalCR;
	}
}
