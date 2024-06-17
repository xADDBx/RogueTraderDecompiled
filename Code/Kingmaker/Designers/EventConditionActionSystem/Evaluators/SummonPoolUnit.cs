using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d67e5007013a92946a7f740132296ab4")]
[PlayerUpgraderAllowed(true)]
public class SummonPoolUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<SummonPoolUnitData>.Current?.Unit;
	}

	public override string GetCaption()
	{
		return "Summon Pool Unit";
	}
}
