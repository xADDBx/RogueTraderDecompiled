using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("54433c6dbef335648b44073fc3f0f06e")]
public class ReloadMechanics : GameAction
{
	public bool ClearFx = true;

	protected override void RunAction()
	{
		Game.ReloadAreaMechanics(ClearFx);
	}

	public override string GetCaption()
	{
		return "Reload mechanic scenes";
	}
}
