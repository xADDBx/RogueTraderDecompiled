using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("4c911278baf34076acb3eddf8daee72c")]
public class ContextConditionAbilityIsRedirected : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		if (base.AbilityContext == null)
		{
			PFLog.Default.Error(this, "Ability for redirection check is missing");
			return false;
		}
		return base.AbilityContext.Ability.IsRedirected;
	}
}
