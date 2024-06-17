using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("acf29654e28cc774eab2764f3d68ef5b")]
public class CheckPassedUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return ContextData<CheckPassedData>.Current?.SkillCheck.Roller;
	}

	public override string GetCaption()
	{
		return "CheckPassedUnit";
	}
}
