using System;
using System.Collections.Generic;
using System.Globalization;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateMomentumLog : TooltipBaseTemplate
{
	private readonly string m_InitiatorName;

	private readonly string m_TargetName;

	private readonly string m_ChangeReason;

	private readonly MomentumChangeReason m_ChangeReasonType;

	private readonly int m_MomentumDeltaValue;

	private readonly int m_MomentumCurrentValue;

	private readonly int m_MomentumMaximumValue;

	private readonly int m_FlatBonus;

	private readonly float m_ResolveLostBase;

	private readonly int m_InitiatorResolve;

	private readonly float m_TargetResolveGained;

	private readonly IEnumerable<Modifier> m_Modifiers;

	public TooltipTemplateMomentumLog(RulePerformMomentumChange evt, string initiatorName, string targetName, string changeReason)
	{
		m_InitiatorName = initiatorName;
		m_TargetName = targetName;
		m_ChangeReason = changeReason;
		m_ChangeReasonType = evt.ChangeReason;
		m_MomentumDeltaValue = evt.ResultDeltaValue;
		m_MomentumCurrentValue = evt.ResultCurrentValue;
		m_FlatBonus = evt.FlatBonus + UnitDifficultyMomentumHelper.GetResolveGainedFlat(evt.MaybeTarget);
		m_InitiatorResolve = evt.ConcreteInitiator.GetStatOptional(StatType.Resolve);
		m_TargetResolveGained = UnitDifficultyMomentumHelper.GetResolveGained(evt.MaybeTarget);
		m_Modifiers = evt.Modifiers.AllModifiersList;
		ContentSpacing = 0f;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string text = UIStrings.Instance.ActionBar.MomentumHeader.Text;
		int momentumCurrentValue = m_MomentumCurrentValue;
		list.Add(new TooltipBrickIconValueStat(text, momentumCurrentValue.ToString(), BlueprintRoot.Instance.UIConfig.UIIcons.TooltipIcons.Momentum, TooltipIconValueStatType.Normal, isWhite: true, needChangeSize: true, 22, 26));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		string text = ((m_MomentumDeltaValue > 0) ? "+" : "");
		int momentumDeltaValue = m_MomentumDeltaValue;
		string momentumValue = "<b>" + text + momentumDeltaValue + "</b>";
		string sourceResolve = string.Format(UIStrings.Instance.CombatLog.MomentumSourceResolve.Text, m_InitiatorName);
		yield return new TooltipBrickIconTextValue("<b>" + UIStrings.Instance.CombatLog.MomentumChangeReason.Text + "</b>", m_ChangeReason, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		yield return new TooltipBrickIconTextValue("<b>" + UIStrings.Instance.CombatLog.MomentumChanged.Text + "</b>", momentumValue, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		float resolveLostBase = UnitDifficultyMomentumHelper.GetResolveLost(UnitDifficultyType.Elite);
		float factor = 0f;
		switch (m_ChangeReasonType)
		{
		case MomentumChangeReason.FallDeadOrUnconscious:
			factor = 0.5f;
			momentumDeltaValue = m_InitiatorResolve;
			yield return new TooltipBrickTextValue(sourceResolve, momentumDeltaValue.ToString() ?? "", 1);
			yield return new TooltipBrickTextValue(UIStrings.Instance.CombatLog.MomentumResolveLostBase.Text, "×" + resolveLostBase, 1);
			break;
		case MomentumChangeReason.KillEnemy:
		{
			momentumDeltaValue = m_InitiatorResolve;
			yield return new TooltipBrickTextValue(sourceResolve, momentumDeltaValue.ToString() ?? "", 1);
			string text2 = string.Format(UIStrings.Instance.CombatLog.MomentumTargetResolveGained.Text, m_TargetName);
			float targetResolveGained = m_TargetResolveGained;
			yield return new TooltipBrickTextValue(text2, "×" + targetResolveGained.ToString(CultureInfo.InvariantCulture), 1);
			break;
		}
		case MomentumChangeReason.StartTurn:
			momentumDeltaValue = m_InitiatorResolve;
			yield return new TooltipBrickTextValue(sourceResolve, momentumDeltaValue.ToString() ?? "", 1);
			break;
		case MomentumChangeReason.Wound:
			factor = 0.25f;
			momentumDeltaValue = m_InitiatorResolve;
			yield return new TooltipBrickTextValue(sourceResolve, momentumDeltaValue.ToString() ?? "", 1);
			yield return new TooltipBrickTextValue(UIStrings.Instance.CombatLog.MomentumResolveLostBase.Text, "×" + resolveLostBase, 1);
			break;
		case MomentumChangeReason.Trauma:
			factor = 0.5f;
			momentumDeltaValue = m_InitiatorResolve;
			yield return new TooltipBrickTextValue(sourceResolve, momentumDeltaValue.ToString() ?? "", 1);
			yield return new TooltipBrickTextValue(UIStrings.Instance.CombatLog.MomentumResolveLostBase.Text, "×" + resolveLostBase, 1);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MomentumChangeReason.Custom:
		case MomentumChangeReason.AbilityCost:
		case MomentumChangeReason.PsychicPhenomena:
			break;
		}
		if (factor != 0f)
		{
			yield return new TooltipBrickTextValue(UIStrings.Instance.CombatLog.MomentumFactor.Text, "×" + factor.ToString(CultureInfo.InvariantCulture), 1);
		}
		foreach (Modifier modifier in m_Modifiers)
		{
			yield return LogThreadBase.CreateBrickModifier(modifier, valueIsPercent: false, null, 2);
		}
		if (m_FlatBonus > 0)
		{
			string text3 = UIStrings.Instance.CombatLog.MomentumFlatBonus.Text;
			momentumDeltaValue = m_FlatBonus;
			yield return new TooltipBrickTextValue(text3, "+" + momentumDeltaValue, 1);
		}
		yield return new TooltipBrickSpace();
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnitInUI.Value;
		int heroicActThreshold = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.HeroicActThreshold;
		int desperateMeasureThreshold = value.GetDesperateMeasureThreshold();
		int maximalMomentum = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.MaximalMomentum;
		yield return new TooltipBrickSlider(maximalMomentum, m_MomentumCurrentValue, new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(maximalMomentum, desperateMeasureThreshold, null, needColor: true, UIConfig.Instance.TooltipColors.ProgressbarPenalty),
			new BrickSliderValueVM(maximalMomentum, heroicActThreshold, null, needColor: true, UIConfig.Instance.TooltipColors.ProgressbarNeutral)
		}, showValue: false, 50, UIConfig.Instance.TooltipColors.ProgressbarBonus);
		yield return new TooltipBrickSpace();
		yield return new TooltipBrickText(UIStrings.Instance.ActionBar.MomentumDescription.Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left, needChangeSize: true);
	}
}
