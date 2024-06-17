using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers.Units;

public class UnitReturnToConsciousController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			PartLifeState lifeState = allUnit.LifeState;
			if (lifeState != null && !allUnit.IsInCombat && !allUnit.Features.DoNotReviveOutOfCombat && !lifeState.IsConscious && !lifeState.IsFinallyDead)
			{
				MakeUnitConscious(allUnit);
			}
		}
	}

	public static void MakeUnitConscious(AbstractUnitEntity unit)
	{
		StatType[] attributes = StatTypeHelper.Attributes;
		foreach (StatType type in attributes)
		{
			ModifiableValueAttributeStat stat = unit.Stats.GetStat<ModifiableValueAttributeStat>(type);
			if (stat.ModifiedValueRaw < 1)
			{
				int num = -stat.ModifiedValueRaw + 1;
				int num2 = Math.Min(stat.Damage, num);
				stat.Damage -= num2;
				num -= num2;
				int num3 = Math.Min(stat.Drain, num);
				stat.Drain -= num3;
			}
		}
		UnitLifeController.ForceUnitConscious(unit);
	}
}
