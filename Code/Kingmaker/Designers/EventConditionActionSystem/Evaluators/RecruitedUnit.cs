using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("6c997aeb1537f2d44a5d7abd8ea1ef0a")]
public class RecruitedUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<RecruitedUnitData>.Current?.Unit;
	}

	public override string GetCaption()
	{
		return "Recruited Unit";
	}
}
