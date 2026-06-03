using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookRollCoverHit : LogThreadBase, IGameLogRuleHandler<RuleRollCoverHit>
{
	public void HandleEvent(RuleRollCoverHit evt)
	{
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.MaybeTarget;
		if (evt.ResultIsHit)
		{
			CombatLogMessage combatLogMessage = LogThreadBase.Strings.WarhammerCoverHit.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.ConcreteInitiator);
			if (evt.AttackRoll != null && evt.AttackRoll.IsOverpenetration)
			{
				combatLogMessage.ReplaceMessage(string.Concat(str1: (!evt.AttackRoll.IsRicochet) ? LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text : LogThreadBase.Strings.TooltipBrickStrings.TriggersRicochet.Text, str0: combatLogMessage.Message));
			}
			AddMessage(combatLogMessage);
		}
	}
}
