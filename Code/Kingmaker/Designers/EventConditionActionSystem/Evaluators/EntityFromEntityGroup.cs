using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("0cc3f0fde5304bb5b35db8ea5129d9fd")]
public class EntityFromEntityGroup : MechanicEntityEvaluator
{
	public override string GetCaption()
	{
		return "Unit Group Unit";
	}

	protected override Entity GetValueInternal()
	{
		return MechanicEntityData.CurrentEntity;
	}
}
