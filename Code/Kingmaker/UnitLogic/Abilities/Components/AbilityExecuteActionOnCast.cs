using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components;

[ComponentName("Ability/ExecuteActions")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("e4181e2a638237a4c9f02ceb97e297b7")]
public class AbilityExecuteActionOnCast : BlueprintComponent, IAbilityOnCastLogic
{
	public ConditionsChecker Conditions;

	public ActionList Actions;

	public void OnCast(AbilityExecutionContext context)
	{
		if (!Conditions.Check())
		{
			return;
		}
		using (context.GetDataScope())
		{
			Actions.Run();
		}
	}
}
