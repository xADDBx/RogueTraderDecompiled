using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("19adcd35ecaa43cdbc3b028092388994")]
public class ControllableState : IntEvaluator
{
	public ControllableReference IdOfObject;

	public override string GetCaption()
	{
		return "State of controllable " + IdOfObject.EntityNameInEditor;
	}

	protected override int GetValueInternal()
	{
		if (!IdOfObject.TryGetValue(out var controllable))
		{
			return 0;
		}
		return Game.Instance.SceneControllables.GetState(controllable.UniqueId).State;
	}
}
