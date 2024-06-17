using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class UnitInitiativeLogThread : LogThreadBase, IGameLogRuleHandler<RuleRollInitiative>
{
	public void HandleEvent(RuleRollInitiative roll)
	{
	}
}
