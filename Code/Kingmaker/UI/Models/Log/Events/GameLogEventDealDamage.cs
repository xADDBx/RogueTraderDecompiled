using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventDealDamage : GameLogEvent<GameLogEventDealDamage>
{
	public RuleDealDamage Damage { get; private set; }

	public static GameLogEventDealDamage Create(RuleDealDamage damage)
	{
		return new GameLogEventDealDamage(damage);
	}

	private GameLogEventDealDamage(RuleDealDamage damage)
	{
		Damage = damage;
	}
}
