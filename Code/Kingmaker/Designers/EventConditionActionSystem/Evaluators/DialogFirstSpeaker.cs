using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("4672b5eaedf80cc4392a6475dfeea78e")]
public class DialogFirstSpeaker : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.DialogController.FirstSpeaker;
	}

	public override string GetCaption()
	{
		return "First Dialog Speaker";
	}
}
