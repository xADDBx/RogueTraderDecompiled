using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("4504f99b2e914f99a61542ee2e3af632")]
public class ReloadUI : GameAction
{
	public override string GetCaption()
	{
		return "Reload UI";
	}

	protected override void RunAction()
	{
		Game.ResetUI();
	}
}
