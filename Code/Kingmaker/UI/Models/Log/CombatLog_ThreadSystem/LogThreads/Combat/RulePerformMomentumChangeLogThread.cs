using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.TextTools;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulePerformMomentumChangeLogThread : LogThreadBase, IGameLogRuleHandler<RulePerformMomentumChange>
{
	public void HandleEvent(RulePerformMomentumChange evt)
	{
		if (evt.ResultGroup != null && evt.ResultGroup.IsParty)
		{
			GameLogContext.MomentumDelta = evt.ResultDeltaValue;
			GameLogContext.MomentumValue = evt.ResultCurrentValue;
			string text = "<b>" + evt.ChangeReason switch
			{
				MomentumChangeReason.Custom => UIStrings.Instance.CombatLog.MomentumTypeCustom.Text, 
				MomentumChangeReason.FallDeadOrUnconscious => UIStrings.Instance.CombatLog.MomentumTypeFallDeadOrUnconscious.Text, 
				MomentumChangeReason.KillEnemy => UIStrings.Instance.CombatLog.MomentumTypeKillEnemy.Text, 
				MomentumChangeReason.StartTurn => UIStrings.Instance.CombatLog.MomentumTypeStartTurn.Text, 
				MomentumChangeReason.AbilityCost => UIStrings.Instance.CombatLog.MomentumTypeAbilityCost.Text, 
				MomentumChangeReason.Wound => UIStrings.Instance.CombatLog.MomentumTypeWound.Text, 
				MomentumChangeReason.Trauma => UIStrings.Instance.CombatLog.MomentumTypeTrauma.Text, 
				_ => null, 
			} + "</b>";
			GameLogContext.Text = text;
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.ConcreteInitiator;
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.MaybeTarget;
			string initiatorName = TextTemplateEngine.Instance.Process("{source}");
			string targetName = TextTemplateEngine.Instance.Process("{target}");
			TooltipTemplateMomentumLog template = new TooltipTemplateMomentumLog(evt, initiatorName, targetName, text);
			MechanicEntity unit = ((evt.ChangeReason == MomentumChangeReason.StartTurn) ? evt.ConcreteInitiator : null);
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.MomentumValueChanged.CreateCombatLogMessage(), template, hasTooltip: true, unit));
		}
	}
}
