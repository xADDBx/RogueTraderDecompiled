using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[AllowMultipleComponents]
[TypeId("ebc9e186f0894144d9c1327dab36124a")]
public class AbilityAreaEffectBuff : AbilityAreaEffectLogic
{
	public ConditionsChecker Condition;

	public bool CheckConditionEveryRound;

	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public bool ReduceAndAddRanks;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		TryApplyBuff(context, areaEffect, unit);
	}

	protected override void OnUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		TryRemoveBuff(context, areaEffect, unit);
	}

	protected override void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!CheckConditionEveryRound)
		{
			return;
		}
		foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
		{
			if (IsConditionPassed(context, item))
			{
				TryApplyBuff(context, areaEffect, item);
			}
			else
			{
				TryRemoveBuff(context, areaEffect, item);
			}
		}
	}

	private void TryApplyBuff(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if ((FindAppliedBuff(areaEffect, unit) == null || ReduceAndAddRanks) && IsConditionPassed(context, unit))
		{
			unit.Buffs.Add(Buff, context)?.AddSource(areaEffect);
		}
	}

	private void TryRemoveBuff(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		Buff buff = FindAppliedBuff(areaEffect, unit);
		if (buff != null)
		{
			if (!ReduceAndAddRanks || buff.Rank == 1)
			{
				buff.Remove();
			}
			if (ReduceAndAddRanks)
			{
				buff.RemoveRank();
			}
		}
	}

	private bool IsConditionPassed(MechanicsContext context, BaseUnitEntity unit)
	{
		using (context.GetDataScope(unit.ToITargetWrapper()))
		{
			return Condition.Check();
		}
	}

	private Buff FindAppliedBuff(AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (!ReduceAndAddRanks)
		{
			return unit.Facts.FindBySource(Buff, areaEffect) as Buff;
		}
		return unit.Buffs.GetBuff(Buff);
	}
}
