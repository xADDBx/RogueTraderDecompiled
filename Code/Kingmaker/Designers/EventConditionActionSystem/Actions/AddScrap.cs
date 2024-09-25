using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("b25ba2e9c1f446e5a7450471a907e54f")]
[PlayerUpgraderAllowed(false)]
public class AddScrap : GameAction
{
	public int Scrap;

	public override string GetCaption()
	{
		return "Give scrap to player";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.Scrap.Receive(Scrap);
	}
}
