using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2cb0d3578ce44f57ac8edc1fb0dd1f57")]
public class UnitsInCombatGetter : PropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public ConditionsChecker Conditions;

	protected override int GetBaseValue()
	{
		List<BaseUnitEntity> list = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat).ToList();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in list)
		{
			using (this.GetMechanicContext().GetDataScope(item))
			{
				if (Conditions.Check())
				{
					list2.Add(item);
				}
			}
		}
		return list2.Count;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of live targetable units in combat";
	}
}
