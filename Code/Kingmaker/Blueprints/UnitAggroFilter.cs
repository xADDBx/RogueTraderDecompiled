using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[ClassInfoBox("Determines if unit should aggro on other unit")]
[TypeId("8e3d2ef65c074a18975ddbf13223b653")]
public class UnitAggroFilter : BlueprintComponent
{
	[InfoBox("No conditions means always should aggro. Otherwise aggroes only if FilterCondition is true")]
	public ConditionsChecker FilterCondition;

	public ActionList ActionsOnAggro;

	public void OnAggroAction(BaseUnitEntity target, BaseUnitEntity attacker)
	{
		if (!ActionsOnAggro.HasActions)
		{
			return;
		}
		using (ContextData<CasterUnitData>.Request().Setup(attacker))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				ActionsOnAggro.Run();
			}
		}
	}

	public bool ShouldAggro(BaseUnitEntity target, BaseUnitEntity attacker)
	{
		if (!FilterCondition.HasConditions)
		{
			return true;
		}
		using (ContextData<CasterUnitData>.Request().Setup(attacker))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				return FilterCondition.Check();
			}
		}
	}
}
