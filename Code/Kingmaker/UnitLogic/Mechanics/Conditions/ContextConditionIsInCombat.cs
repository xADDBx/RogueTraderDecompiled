using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("ae19c598329d40e48a63b182383f8f97")]
public class ContextConditionIsInCombat : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check if target is in combat";
	}

	protected override bool CheckCondition()
	{
		return base.Target.Entity?.IsInCombat ?? false;
	}
}
