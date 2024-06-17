using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c291eff3e32794044804e2ba104df165")]
public class PartyMembersSwapAttachedAndDetached : GameAction
{
	public override string GetCaption()
	{
		return "Switch attached and detached party members";
	}

	public override void RunAction()
	{
		Game.Instance.Player.SwapAttachedAndDetachedPartyMembers();
	}
}
