using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Base;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RollSkillCheckLogThread : BaseRollSkillCheckLogThread
{
	public override void HandleEvent(RulePerformPartySkillCheck check)
	{
		if (check.SkinningTarget == null)
		{
			base.HandleEvent(check);
		}
	}

	public override void HandleEvent(RulePerformSkillCheck check)
	{
		if ((check.StatType != StatType.SkillAwareness || check.ResultIsSuccess || Game.Instance.Player.UISettings.ShowFailedPerceptionChecks || check.ShowAnyway) && check.SkinningTarget == null)
		{
			base.HandleEvent(check);
		}
	}
}
