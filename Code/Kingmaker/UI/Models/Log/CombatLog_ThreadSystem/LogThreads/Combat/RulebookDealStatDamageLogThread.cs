using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookDealStatDamageLogThread : LogThreadBase, IGameLogRuleHandler<RuleDealStatDamage>
{
	public void HandleEvent(RuleDealStatDamage rule)
	{
		StatDamageLogMessage statDamageLogMessage = (rule.IsDrain ? LogThreadBase.Strings.StatDrain : LogThreadBase.Strings.StatDamage);
		AddMessage(statDamageLogMessage.GetData(rule, rule.Stat.Type, rule.Result));
	}
}
