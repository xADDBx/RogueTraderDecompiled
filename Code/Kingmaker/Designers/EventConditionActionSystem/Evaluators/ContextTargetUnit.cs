using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("cf2430390b1b475490921b4ba97a682f")]
public class ContextTargetUnit : AbstractUnitEvaluator
{
	public override string GetCaption()
	{
		return "Target unit from mechanic context";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.CurrentTarget.Entity as BaseUnitEntity;
	}
}
