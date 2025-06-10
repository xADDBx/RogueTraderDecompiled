using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Selection;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;

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
		if (UIAccess.SelectionManager is SelectionManagerPC selectionManagerPC)
		{
			List<UnitEntityView> views = Game.Instance.Player.Party.Select((BaseUnitEntity character) => character.View).ToTempList();
			selectionManagerPC.MultiSelect(views);
		}
	}
}
