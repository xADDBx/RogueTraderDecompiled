using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Dialog;

public class DialogHistoryLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventDialogHistory>
{
	public void HandleEvent(GameLogEventDialogHistory evt)
	{
		DialogColors dialogColors = BlueprintRoot.Instance.UIConfig.DialogColors;
		AddMessage(new CombatLogMessage(evt.GetText(dialogColors), default(Color), PrefixIcon.None, null, hasTooltip: false));
	}
}
