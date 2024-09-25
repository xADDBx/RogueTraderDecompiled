using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("e8a060efc7f39c94c955f0bc89c81104")]
public class Spotter : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<SpotterData>.Current?.Unit;
	}

	public override string GetCaption()
	{
		return "Clicked Unit";
	}
}
