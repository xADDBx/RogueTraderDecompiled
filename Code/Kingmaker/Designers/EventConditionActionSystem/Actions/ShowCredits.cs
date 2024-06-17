using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("76367edfd4564f4d86bd2372754cb476")]
public class ShowCredits : GameAction
{
	public override string GetCaption()
	{
		return "Show Credits";
	}

	public override void RunAction()
	{
		EventBus.RaiseEvent(delegate(ICreditsWindowUIHandler h)
		{
			h.HandleOpenCredits();
		});
	}
}
