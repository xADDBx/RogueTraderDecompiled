using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("56703b87cc0c36347937d201cc076a6d")]
public class CasterUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<MechanicsContext.Data>.Current?.Context.MaybeCaster as BaseUnitEntity;
	}

	public override string GetCaption()
	{
		return "Caster Unit";
	}
}
