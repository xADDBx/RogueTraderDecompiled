using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("437e91be60e97ae4cbd13c47f2b2de18")]
public class DialogCurrentSpeaker : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.DialogController.CurrentSpeaker;
	}

	public override string GetCaption()
	{
		return "Current Dialog Speaker";
	}
}
