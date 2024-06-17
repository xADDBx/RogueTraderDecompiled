using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class GameLogMessage
{
	public Color32 Color;

	public PrefixIcon Icon = PrefixIcon.None;

	public LocalizedString Message;

	public LocalizedString Tooltip;

	[CanBeNull]
	public CombatLogMessage CreateCombatLogMessage(TooltipBaseTemplate tooltipTemplate = null, string tooltipHeader = null, bool isPerformAttackMessage = false, MechanicEntity unit = null)
	{
		try
		{
			using (ProfileScope.New("Build Log Message"))
			{
				using (GameLogContext.Scope)
				{
					if (string.IsNullOrEmpty(Message))
					{
						return null;
					}
					tooltipHeader = (tooltipHeader.IsNullOrEmpty() ? Message.Text : tooltipHeader);
					TooltipBaseTemplate template = tooltipTemplate ?? new TooltipTemplateCombatLogMessage(tooltipHeader, Tooltip.Text);
					PrefixIcon icon = ((Icon != PrefixIcon.None) ? Icon : GameLogContext.GetIcon());
					return new CombatLogMessage(Message.Text, GetColor(), icon, template, hasTooltip: true, 0, isSeparator: false, GameLogEventAddSeparator.States.Finish, isPerformAttackMessage, unit);
				}
			}
		}
		finally
		{
			StatModifiersBreakdown.Clear();
		}
	}

	[CanBeNull]
	public CombatLogMessage CreateCombatLogMessage(bool isSeparator, GameLogEventAddSeparator.States separatorState)
	{
		try
		{
			using (ProfileScope.New("Build Log Message"))
			{
				using (GameLogContext.Scope)
				{
					return new CombatLogMessage(Message, isSeparator, separatorState);
				}
			}
		}
		finally
		{
			StatModifiersBreakdown.Clear();
		}
	}

	protected Color32 GetColor()
	{
		return Multiply((Color.r > 0 || Color.g > 0 || Color.b > 0 || Color.a > 0) ? Color : GameLogStrings.Instance.DefaultColor, GameLogStrings.Instance.ColorMultiplier);
	}

	public static Color32 Multiply(Color32 a, Color32 b)
	{
		a.r = (byte)(a.r * b.r >> 8);
		a.g = (byte)(a.g * b.g >> 8);
		a.b = (byte)(a.b * b.b >> 8);
		a.a = (byte)(a.a * b.a >> 8);
		return a;
	}
}
