using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class RulebookRollChanceLogThread : LogThreadBase, IGameLogRuleHandler<RuleRollChance>
{
	public void HandleEvent(RuleRollChance rule)
	{
		if (rule.Type == RollChanceType.AffectedByShadow)
		{
			string message = (rule.Success ? $"[TEST] Shadow effect occur. Chance {rule.Chance}, roll {rule.Result}" : $"[TEST] Shadow effect ignored. Chance {rule.Chance}, roll {rule.Result}");
			AddMessage(new CombatLogMessage(message, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None));
		}
	}
}
