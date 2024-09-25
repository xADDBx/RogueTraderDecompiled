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
		ControllableState state = new ControllableState
		{
			Active = Active
		};
		Game.Instance.SceneControllables.SetState(IdOfObject.UniqueId, state);
	}
}
