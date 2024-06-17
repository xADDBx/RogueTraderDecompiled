using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Kingmaker.UI.Models.Log.Enums;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickNestedMessageVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly Color TextColor;

	public readonly PrefixIcon PrefixIcon;

	public readonly int ShotNumber;

	public readonly MechanicEntity Unit;

	public readonly TooltipBaseTemplate TooltipTemplate;

	public readonly bool NeedShowLine;

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public TooltipBrickNestedMessageVM(string text, Color textColor, PrefixIcon prefixIcon, int shotNumber, MechanicEntity unit, TooltipBaseTemplate tooltipTemplate, bool needShowLine = true)
	{
		Text = text;
		TextColor = textColor;
		PrefixIcon = prefixIcon;
		ShotNumber = shotNumber;
		Unit = unit;
		TooltipTemplate = tooltipTemplate;
		NeedShowLine = needShowLine;
	}
}
