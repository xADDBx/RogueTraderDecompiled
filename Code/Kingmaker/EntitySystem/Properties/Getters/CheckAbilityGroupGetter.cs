using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
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
		AbilityData ability = this.GetAbility();
		if (!(ability != null))
		{
			return 0;
		}
		if (Groups.FirstOrDefault(ability.Blueprint.AbilityGroups.HasReference) == null)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "AbilityGroup is " + string.Join("|", from i in Groups
			where i != null
			select i.ToString());
	}
}
