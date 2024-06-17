using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("622b2e3118c34edebe1597cf4c03339e")]
public class CheckAbilityBlueprintGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintAbilityReference[] m_Abilities;

	public ReferenceArrayProxy<BlueprintAbility> Abilities
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] abilities = m_Abilities;
			return abilities;
		}
	}

	protected override int GetBaseValue()
	{
		if (!Abilities.HasReference(this.GetAbility()?.Blueprint))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "Ability is " + string.Join("|", from i in Abilities
			where i != null
			select i.ToString());
	}
}
