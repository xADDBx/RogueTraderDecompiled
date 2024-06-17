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
			AddMessage(LogThreadBase.Strings.WarhammerCoverHit.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, evt.ConcreteInitiator));
		}
	}
}
