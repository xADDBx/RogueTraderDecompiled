using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("48b8a781058349844b7161754716a9bb")]
public class DialogActingUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.DialogController.ActingUnit;
	}

	public override string GetCaption()
	{
		return "Dialog acting unit";
	}
}
