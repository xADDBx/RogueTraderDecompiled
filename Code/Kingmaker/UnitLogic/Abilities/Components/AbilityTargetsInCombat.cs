using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("7dbea4fb2b4f5da4ebd57cd9d17967c2")]
public class AbilityTargetsInCombat : AbilitySelectTarget
{
	[SerializeField]
	private TargetType m_TargetType;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private ConditionsChecker m_Condition;

	public TargetType Targets => m_TargetType;

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		MechanicEntity caster = context.MaybeCaster;
		if (caster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return Enumerable.Empty<TargetWrapper>();
		}
		IEnumerable<BaseUnitEntity> source = Game.Instance.State.AllBaseAwakeUnits.Where(delegate(BaseUnitEntity x)
		{
			if (m_IncludeCaster)
			{
				return x.IsInCombat;
			}
			return x != caster && x.IsInCombat;
		});
		switch (Targets)
		{
		case TargetType.Enemy:
			source = source.Where(caster.IsEnemy);
			break;
		case TargetType.Ally:
			source = source.Where(caster.IsAlly);
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
}
