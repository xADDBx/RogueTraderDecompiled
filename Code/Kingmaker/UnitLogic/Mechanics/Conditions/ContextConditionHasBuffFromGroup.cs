using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("de3d2f721aba4974b2299ec267e724c1")]
public class ContextConditionHasBuffFromGroup : ContextCondition
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public bool OnlyFromCaster;

	public bool CheckOnCaster;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override string GetConditionCaption()
	{
		return string.Concat("Check if target has buff from group list");
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity mechanicEntity = (CheckOnCaster ? base.Context.MaybeCaster : base.Target.Entity);
		if (mechanicEntity == null)
		{
			return false;
		}
		foreach (Buff buff in mechanicEntity.Buffs)
		{
			if (buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup p) => Groups.Contains(p)) && (!OnlyFromCaster || buff.Context.MaybeCaster == maybeCaster))
			{
				return true;
			}
		}
		return false;
	}
}
