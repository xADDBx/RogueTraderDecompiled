using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("c7ad576e81e4478a83e2cb2a3814a49f")]
public class HasBuffsFromGroupGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public bool OnlyFromEntity;

	[ShowIf("OnlyFromEntity")]
	public PropertyTargetType Target;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override int GetBaseValue()
	{
		if (!(base.CurrentEntity is UnitEntity unitEntity))
		{
			return 0;
		}
		if (OnlyFromEntity)
		{
			BaseUnitEntity caster = this.GetTargetByType(Target) as BaseUnitEntity;
			if (!unitEntity.Buffs.Enumerable.Where((Buff buffCheck) => buffCheck.Context.MaybeCaster == caster).Any((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => Groups.Contains(group))))
			{
				return 0;
			}
			return 1;
		}
		if (!unitEntity.Buffs.Enumerable.Any((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => Groups.Contains(group))))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "Has buffs from " + string.Join("|", from i in Groups
			where i != null
			select i.ToString());
	}
}
