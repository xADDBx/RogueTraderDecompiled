using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UI;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("cce2591ce52140528f7cb47bb3ad5a18")]
public class SelectAllActivePartyUnits : GameAction
{
	public override string GetCaption()
	{
		return "Select all active party units";
	}

	protected override void RunAction()
	{
		UIAccess.SelectionManager?.SelectAll();
	}
}
