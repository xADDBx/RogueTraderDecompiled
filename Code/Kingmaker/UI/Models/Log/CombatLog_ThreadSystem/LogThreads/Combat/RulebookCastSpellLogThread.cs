using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Base;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookCastSpellLogThread : BaseUseAbilityLogThread, IGameLogRuleHandler<RulePerformAbility>
{
	public void HandleEvent(RulePerformAbility rule)
	{
		if (rule == null || !rule.Context.DisableLog)
		{
			HandleUseAbility(rule.Spell, rule);
		}
	}
}
