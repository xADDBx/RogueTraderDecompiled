using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f6b0e0b15d534b5fb20168697144055c")]
public class BlockServiceWindows : GameAction
{
	public bool Unblock;

	public override string GetCaption()
	{
		return (Unblock ? "Unblock" : "Block") + " Service UI windows";
	}

	protected override void RunAction()
	{
		if (Unblock)
		{
			Game.Instance.Player.ServiceWindowsBlocked.Release();
		}
		else
		{
			Game.Instance.Player.ServiceWindowsBlocked.Retain();
		}
		EventBus.RaiseEvent(delegate(ICanAccessServiceWindowsHandler h)
		{
			h.HandleServiceWindowsBlocked();
		});
	}
}
