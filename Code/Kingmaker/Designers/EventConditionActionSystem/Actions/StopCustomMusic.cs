using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("fc8c0dda384d425087138a6644060968")]
public class StopCustomMusic : GameAction
{
	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return "Stop custom music";
	}
}
