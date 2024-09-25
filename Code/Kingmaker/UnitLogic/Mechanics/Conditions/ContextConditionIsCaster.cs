using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("7a10e867e7cb49e09b80e164d151f656")]
public class ContextConditionIsCaster : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Is caster";
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity != null)
		{
			return base.Target == base.Context.MaybeCaster;
		}
		return false;
	}
}
