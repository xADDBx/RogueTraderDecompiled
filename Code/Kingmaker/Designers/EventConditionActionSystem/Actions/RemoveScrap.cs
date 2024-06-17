using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("fa408806425b4c56a0bc5c7770a2ff60")]
[PlayerUpgraderAllowed(false)]
public class RemoveScrap : GameAction
{
	public int Scrap;

	public override string GetCaption()
	{
		return "Take scrap from player";
	}

	public override void RunAction()
	{
		Game.Instance.Player.Scrap.Spend(Scrap);
	}
}
