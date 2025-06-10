using System.Collections;
using System.Collections.Generic;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.QA.Clockwork;

public class TaskHeal : ClockworkRunnerTask
{
	public TaskHeal(ClockworkRunner runner)
		: base(runner)
	{
	}

	protected override IEnumerator Routine()
	{
		yield return 1f;
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.LifeState.IsDead)
			{
				partyAndPet.LifeState.Resurrect();
			}
			RemoveAllBuffs(partyAndPet);
			GameHelper.HealDamage(partyAndPet, partyAndPet, partyAndPet.Health.Damage);
		}
		yield return 1f;
	}

	private void RemoveAllBuffs(BaseUnitEntity unit)
	{
		foreach (Buff item in new List<Buff>(unit.Buffs.Enumerable))
		{
			if (item.Blueprint != Game.Instance.BlueprintRoot.Cheats.Iddqd)
			{
				unit.Buffs.RawFacts.Remove(item);
			}
		}
	}

	public override bool TooManyAttempts()
	{
		return false;
	}

	public override string ToString()
	{
		return "Heal up";
	}
}
