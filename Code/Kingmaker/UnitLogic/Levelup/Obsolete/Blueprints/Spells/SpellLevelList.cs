using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.DLC;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[Serializable]
public class SpellLevelList
{
	public int SpellLevel;

	[SerializeField]
	[FormerlySerializedAs("Spells")]
	private List<BlueprintAbilityReference> m_Spells = new List<BlueprintAbilityReference>();

	private ReferenceListProxy<BlueprintAbility, BlueprintAbilityReference> m_SpellsProxy;

	private List<BlueprintAbility> m_SpellsFiltered;

	public IList<BlueprintAbility> Spells
	{
		get
		{
			ReferenceListProxy<BlueprintAbility, BlueprintAbilityReference> obj = m_SpellsProxy ?? ((ReferenceListProxy<BlueprintAbility, BlueprintAbilityReference>)m_Spells);
			ReferenceListProxy<BlueprintAbility, BlueprintAbilityReference> result = obj;
			m_SpellsProxy = obj;
			return result;
		}
	}

	public List<BlueprintAbility> SpellsFiltered
	{
		get
		{
			if (m_SpellsFiltered == null || m_SpellsFiltered.Count == 0)
			{
				m_SpellsFiltered = m_SpellsFiltered ?? new List<BlueprintAbility>();
				m_SpellsFiltered.AddRange(Spells.Where((BlueprintAbility spell) => !spell.IsDlcRestricted()));
			}
			return m_SpellsFiltered;
		}
	}

	public SpellLevelList(int spellLevel)
	{
		SpellLevel = spellLevel;
	}
}
