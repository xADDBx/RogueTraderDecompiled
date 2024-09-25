using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;

public class SpaceCombatMovementActionHolder : BaseDisposable, ISpaceCombatActionsHolder
{
	private static float CurrentUnitAP => (Game.Instance.TurnController.CurrentUnit is BaseUnitEntity baseUnitEntity) ? Mathf.RoundToInt(baseUnitEntity.CombatState.ActionPointsBlue) : 0;

	protected override void DisposeImplementation()
	{
	}

	public bool HasActions()
	{
		return CurrentUnitAP > 0f;
	}

	public void HighlightOn()
	{
		string warningText = string.Format(UIStrings.Instance.SpaceCombatTexts.CombatMovementActionHint, CurrentUnitAP);
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(warningText, addToLog: false, WarningNotificationFormat.Attention);
		});
	}

	public void HighlightOff()
	{
	}
}
