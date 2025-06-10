using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("33c67d2b2ea84901bfa037419a557cca")]
public class ContextClickedTargetUnit : AbstractUnitEvaluator
{
	public override string GetCaption()
	{
		return "Clicked unit from AbilityExecutionContext context";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<AbilityExecutionContext.Data>.Current?.ClickedTarget?.Entity as BaseUnitEntity;
	}
}
