using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookHealStatDamageLogThread : LogThreadBase, IGameLogRuleHandler<RuleHealStatDamage>
{
	public void HandleEvent(RuleHealStatDamage rule)
	{
		if (rule.HealedDamage > 0)
		{
			AddMessage(LogThreadBase.Strings.HealStatDamage.GetData(rule, rule.StatType, rule.HealedDamage));
		}
		if (rule.HealedDrain > 0)
		{
			AddMessage(LogThreadBase.Strings.HealStatDrain.GetData(rule, rule.StatType, rule.HealedDrain));
		}
	}
}
