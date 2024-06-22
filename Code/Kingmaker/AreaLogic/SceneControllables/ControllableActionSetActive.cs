using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.AreaLogic.SceneControllables;

[PlayerUpgraderAllowed(false)]
[TypeId("72dccdfcda9347f19b578730b030b8b5")]
public class ControllableActionSetActive : GameAction
{
	public bool Active;

	public ControllableReference IdOfObject;

	public override string GetCaption()
	{
		return "Set " + IdOfObject.EntityNameInEditor + " " + (Active ? "Active" : "Inactive");
	}

	protected override void RunAction()
	{
		if (!IdOfObject.TryGetValue(out var controllable))
		{
			Game.Instance.SceneControllables.SetState(controllable.UniqueId, new ControllableState
			{
				Active = Active
			});
		}
		else
		{
			ControllableState state = Game.Instance.SceneControllables.GetState(controllable.UniqueId);
			state.Active = Active;
			Game.Instance.SceneControllables.SetState(controllable.UniqueId, state);
		}
	}
}
