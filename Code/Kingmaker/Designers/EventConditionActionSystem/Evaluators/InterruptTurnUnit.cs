using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("95193f7cb4e6465892c22b716147d2b3")]
public class InterruptTurnUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return (AbstractUnitEntity)(ContextData<InterruptTurnData>.Current?.Unit);
	}

	public override string GetCaption()
	{
		return "Interrupt turn unit";
	}
}
