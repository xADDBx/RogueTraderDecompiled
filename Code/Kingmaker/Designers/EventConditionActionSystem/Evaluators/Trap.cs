using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("2b7fcc4e2535dc24a94f59da3723957b")]
public class Trap : MapObjectEvaluator
{
	protected override MapObjectEntity GetMapObjectInternal()
	{
		return ContextData<BlueprintTrap.ElementsData>.Current?.TrapObject.Data;
	}

	public override string GetCaption()
	{
		return "Trap";
	}
}
