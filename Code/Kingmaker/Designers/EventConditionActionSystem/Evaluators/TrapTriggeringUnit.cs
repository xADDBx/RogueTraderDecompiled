using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("ba2fa159998dafa4cb669cd5583da0ad")]
public class TrapTriggeringUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<BlueprintTrap.ElementsData>.Current?.TriggeringUnit;
	}

	public override string GetCaption()
	{
		return "Whoever triggered the trap";
	}
}
