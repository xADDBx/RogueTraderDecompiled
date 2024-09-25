using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class GrenadeDealDamageLogThread : LogThreadBase, IGameLogRuleHandler<RuleDealDamage>, IGameLogEventHandler<GameLogEventAttack>
{
	private GameLogEventAttack m_GameLogEventAttack;

	void IGameLogRuleHandler<RuleDealDamage>.HandleEvent(RuleDealDamage evt)
	{
		if (!(evt.SourceAbility == null) && evt.SourceAbility.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			if (m_GameLogEventAttack == null)
			{
				Show(evt);
			}
			else if (m_GameLogEventAttack?.Rule.ResultDamageRule != null)
			{
				Show(evt);
			}
			else if (!(m_GameLogEventAttack.Rule.Ability != evt.SourceAbility) && m_GameLogEventAttack.Rule.Initiator == evt.Initiator && m_GameLogEventAttack.Rule.Target == evt.Target)
			{
				Show(m_GameLogEventAttack, evt);
				m_GameLogEventAttack = null;
			}
		}
	}

	void IGameLogEventHandler<GameLogEventAttack>.HandleEvent(GameLogEventAttack evt)
	{
		RulePerformAttack rule = evt.Rule;
		if (!(rule.Ability == null) && rule.Ability.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			if (rule.ResultDamageRule != null)
			{
				Show(evt);
			}
			else
			{
				m_GameLogEventAttack = evt;
			}
		}
	}

	private void Show(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null)
	{
		AddMessage(PerformAttackLogThread.CreateMessage(evt, overrideDealDamage));
	}

	private void Show(RuleDealDamage evt)
	{
		AddMessage(RulebookDealDamageLogThread.CreateMessage(evt));
	}
}
