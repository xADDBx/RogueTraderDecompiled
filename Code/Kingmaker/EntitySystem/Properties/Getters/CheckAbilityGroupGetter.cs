using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("6a45c42d96204b7d930bd97fa922b35d")]
public class CheckAbilityGroupGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

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
		if (!(this.GetAbility() != null))
		{
			return 0;
		}
		if (Groups.FirstOrDefault(this.GetAbility().Blueprint.AbilityGroups.HasReference) == null)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "AbilityGroup is " + string.Join("|", from i in Groups
			where i != null
			select i.ToString());
	}
}
