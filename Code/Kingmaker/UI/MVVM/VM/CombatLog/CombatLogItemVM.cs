using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Enums;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CombatLog;

public class CombatLogItemVM : CombatLogBaseVM
{
	public readonly string MessageText;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly int ShotNumber;

	public readonly MechanicEntity Unit;

	public readonly TooltipBaseTemplate TooltipTemplate;

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CombatLogItemVM(CombatLogMessage message)
		: base(message)
	{
		MessageText = message.Message;
		TextColor = message.TextColor;
		PrefixIcon = message.PrefixIcon;
		ShotNumber = message.ShotNumber;
		Unit = message.Unit;
		TooltipTemplate = message.Tooltip;
	}
}
