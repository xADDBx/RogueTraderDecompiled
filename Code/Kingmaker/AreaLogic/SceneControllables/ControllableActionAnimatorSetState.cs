using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.AreaLogic.SceneControllables;

[PlayerUpgraderAllowed(false)]
[TypeId("069b9553e9bf4011ad8e4a10e67e805c")]
public class ControllableActionAnimatorSetState : GameAction
{
	public int State;

	public ControllableAnimatorSetStateReference IdOfObject;

	public override string GetCaption()
	{
		return $"Set {IdOfObject?.EntityNameInEditor} Animator State to {State}";
	}

	protected override void RunAction()
	{
		ControllableState state = new ControllableState
		{
			State = State
		};
		Game.Instance.SceneControllables.SetState(IdOfObject?.UniqueId, state);
	}
}
