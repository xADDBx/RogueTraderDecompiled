using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[TypeId("757439ef8cc900741bd9b57bf26eb500")]
public class AbilityTargetsAround : AbilitySelectTarget, IAbilityAoERadiusProvider
{
	[SerializeField]
	private int m_Radius;

	[SerializeField]
	private TargetType m_TargetType;

	[SerializeField]
	private bool m_IncludeDead;

	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	private Feet m_SpreadSpeed;

	public bool InWeaponRange;

	public int AoERadius => m_Radius;

	public TargetType Targets => m_TargetType;

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		Vector3 point = (anchor.HasEntity ? anchor.Entity.EyePosition : anchor.Point);
		IEnumerable<BaseUnitEntity> source = ((!InWeaponRange) ? GameHelper.GetTargetsAround(point, m_Radius, checkLOS: true, m_IncludeDead) : GameHelper.GetTargetsAround(point, context.MaybeCaster.GetFirstWeapon().AttackRange, checkLOS: true, m_IncludeDead));
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return Enumerable.Empty<TargetWrapper>();
		}
		switch (Targets)
		{
		case TargetType.Enemy:
			source = source.Where(context.Caster.IsEnemy);
			break;
		case TargetType.Ally:
			source = source.Where(context.Caster.IsAlly);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case TargetType.Any:
			break;
		}
		if (m_Condition.HasConditions)
		{
			source = source.Where(delegate(BaseUnitEntity u)
			{
				using (context.GetDataScope(u.ToITargetWrapper()))
				{
					return m_Condition.Check();
				}
			}).ToList();
		}
		return source.Select((BaseUnitEntity u) => new TargetWrapper(u));
	}

	public override float? GetSpreadSpeedMps()
	{
		if (!((double)m_SpreadSpeed.Meters <= 1E-06))
		{
			return m_SpreadSpeed.Meters;
		}
		return null;
	}

	public bool WouldTargetUnit(AbilityData ability, Vector3 targetPos, BaseUnitEntity unit)
	{
		return unit.IsUnitInRangeCells(targetPos, AoERadius);
	}
}
