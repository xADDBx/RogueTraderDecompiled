using System.Collections.Generic;
using System.Linq;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class HealingLogThread : LogThreadBase, IGameLogRuleHandler<RuleHealDamage>
{
	public void HandleEvent(RuleHealDamage rule)
	{
		if (rule.ConcreteTarget.IsInGame)
		{
			CombatLogMessage data = LogThreadBase.Strings.HealDamage.GetData(rule.CalculateHealRule);
			if (data?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
			{
				tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(rule).ToArray();
				tooltipTemplateCombatLogMessage.ExtraInfoBricks = CollectExtraBricks(rule).ToArray();
			}
			AddMessage(data);
		}
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleHealDamage rule)
	{
		yield return new TooltipBrickIconTextValue(LogThreadBase.Strings.TooltipBrickStrings.Initiator.Text, rule.ConcreteInitiator.Name, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		yield return new TooltipBrickIconTextValue(LogThreadBase.Strings.TooltipBrickStrings.HealsWounds.Text, rule.Value.ToString(), 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
	}
}
