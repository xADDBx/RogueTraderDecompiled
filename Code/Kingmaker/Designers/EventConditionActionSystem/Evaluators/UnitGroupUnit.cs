using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("108c71e06af64dc8a69cb1290889fb18")]
public class UnitGroupUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		UnitsFromSpawnersInUnitGroup.UnitData current = ContextData<UnitsFromSpawnersInUnitGroup.UnitData>.Current;
		if (current == null)
		{
			return ContextData<SpawnedUnitData>.Current?.Unit;
		}
		return current.Unit;
	}

	public override string GetCaption()
	{
		return "Unit Group Unit";
	}
}
