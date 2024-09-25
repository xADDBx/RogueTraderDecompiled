using System.Collections;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.QA.Clockwork;

public class TaskWinCombat : ClockworkRunnerTask
{
	public TaskWinCombat(ClockworkRunner runner)
		: base(runner)
	{
	}

	protected override IEnumerator Routine()
	{
		BaseUnitEntity player = Runner.Player;
		while (player.IsInCombat)
		{
			yield return 5f;
			if (!Clockwork.Instance.Scenario.CheaterCombat)
			{
				continue;
			}
			foreach (UnitGroupMemory.UnitInfo enemy in player.CombatGroup.Memory.Enemies)
			{
				if (!(enemy?.Unit?.LifeState.IsDead ?? true))
				{
					Game.Instance.Rulebook.TriggerEvent(new RuleDealDamage(player, enemy.Unit, new DamageData(DamageType.Direct, 20)));
				}
			}
			if (Game.Instance.Player.Party.Any((BaseUnitEntity u) => u.Health.Damage > u.Health.HitPoints.ModifiedValue / 2 || u.LifeState.IsDead))
			{
				yield return new TaskHeal(Runner);
			}
		}
	}

	public override bool TooManyAttempts()
	{
		return false;
	}

	public override string ToString()
	{
		return "Win this combat";
	}
}
