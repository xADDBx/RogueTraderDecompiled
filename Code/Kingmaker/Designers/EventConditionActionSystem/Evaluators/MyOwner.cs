using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("128b24c135cf4c5b97bbe868981c9c4c")]
public class MyOwner : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ((ContextData<PropertyContextData>.Current?.Context.CurrentEntity as BaseUnitEntity) ?? (ContextData<MechanicsContext.Data>.Current?.Context.MaybeCaster as BaseUnitEntity))?.Master;
	}

	public override string GetCaption()
	{
		return "Get my Owner";
	}
}
