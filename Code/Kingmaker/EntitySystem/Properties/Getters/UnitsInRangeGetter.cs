using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("e5ea156d7248471887237e2c871de79a")]
public class UnitsInRangeGetter : PropertyGetter, PropertyContextAccessor.ICurrentTarget, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	private enum UnitCombatGroup
	{
		Ally = 1,
		Enemy = 2
	}

	private const float HalfDiagonal = 0.95f;

	[SerializeField]
	private PropertyCalculator m_Value = new PropertyCalculator();

	[SerializeField]
	private UnitCombatGroup m_CombatGroup;

	[SerializeField]
	private int m_RangeInCells;

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error(this, "Target is missing");
			return 0;
		}
		float num = ((m_RangeInCells == 1) ? 0.95f : 0f);
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(baseUnitEntity.Position, m_RangeInCells.Cells().Meters + num);
		int num2 = 0;
		foreach (BaseUnitEntity item in list)
		{
			if (item.UniqueId != baseUnitEntity.UniqueId && item.LifeState.IsConscious && IsSuitableCombatGroup(item, baseUnitEntity) && (m_Value.Empty || m_Value.GetBoolValue(base.PropertyContext.WithCurrentEntity(item))))
			{
				num2++;
			}
		}
		return num2;
	}

	private bool IsSuitableCombatGroup(BaseUnitEntity unit, BaseUnitEntity target)
	{
		if (m_CombatGroup.HasFlag(UnitCombatGroup.Ally) && m_CombatGroup.HasFlag(UnitCombatGroup.Enemy))
		{
			return true;
		}
		if (m_CombatGroup != UnitCombatGroup.Ally || unit.IsEnemy(target))
		{
			if (m_CombatGroup == UnitCombatGroup.Enemy)
			{
				return unit.IsEnemy(target);
			}
			return false;
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (m_CombatGroup.HasFlag(UnitCombatGroup.Ally) && m_CombatGroup.HasFlag(UnitCombatGroup.Enemy))
		{
			return $"Amount of units in range of {m_RangeInCells} cells around {FormulaTargetScope.Current}";
		}
		return string.Format("Amount of {0} units in range of {1} cells around {2}", m_CombatGroup switch
		{
			UnitCombatGroup.Ally => "Ally", 
			UnitCombatGroup.Enemy => "Enemy", 
			_ => "NONE SELECTED", 
		}, m_RangeInCells, FormulaTargetScope.Current);
	}
}
