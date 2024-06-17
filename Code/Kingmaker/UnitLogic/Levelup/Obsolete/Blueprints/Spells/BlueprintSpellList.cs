using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[TypeId("1585acbfcc93f0a439fa39dde12070c3")]
public class BlueprintSpellList : BlueprintScriptableObject, IBlueprintScanner
{
	public bool IsMythic;

	[NotNull]
	[ArrayElementNamePrefix("Spell Level", false)]
	public SpellLevelList[] SpellsByLevel = new SpellLevelList[0];

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("FilteredList")]
	private BlueprintSpellListReference m_FilteredList;

	public int FilterByMaxLevel;

	public bool FilterByDescriptor;

	[ShowIf("FilterByDescriptor")]
	[InfoBox("Add only spells with any specified descriptors")]
	public SpellDescriptorWrapper Descriptor;

	public bool FilterBySchool = true;

	[ShowIf("FilterBySchool")]
	public bool ExcludeFilterSchool;

	[ShowIf("FilterBySchool")]
	public SpellSchool FilterSchool;

	[ShowIf("FilterBySchool")]
	public SpellSchool FilterSchool2;

	private int m_MaxLevel;

	private static readonly List<BlueprintAbility> EmptyAbilitiesList = new List<BlueprintAbility>();

	public BlueprintSpellList FilteredList => m_FilteredList?.Get();

	public int MaxLevel
	{
		get
		{
			if (m_MaxLevel <= 0)
			{
				return m_MaxLevel = SpellsByLevel.Where((SpellLevelList s) => s.SpellsFiltered.Count > 0).Max((SpellLevelList s) => s.SpellLevel);
			}
			return m_MaxLevel;
		}
	}

	public List<BlueprintAbility> GetSpells(int spellLevel)
	{
		if (spellLevel < 0 || spellLevel >= SpellsByLevel.Length)
		{
			return EmptyAbilitiesList;
		}
		return SpellsByLevel[spellLevel].SpellsFiltered;
	}

	public bool Contains(BlueprintAbility spell)
	{
		return SpellsByLevel.Any((SpellLevelList level) => level.SpellsFiltered.Contains(spell));
	}

	public int GetLevel(BlueprintAbility spell)
	{
		SpellLevelList[] spellsByLevel = SpellsByLevel;
		foreach (SpellLevelList spellLevelList in spellsByLevel)
		{
			if (spellLevelList.SpellsFiltered.Contains(spell))
			{
				return spellLevelList.SpellLevel;
			}
		}
		return -1;
	}

	public void Scan()
	{
	}

	private bool CanAddSpell(int spellLevel, BlueprintAbility spell)
	{
		return false;
	}

	private bool DescriptorAllowed(BlueprintAbility spell)
	{
		return false;
	}

	private bool SchoolAllowed(BlueprintAbility spell)
	{
		return false;
	}

	private void AddSpell(int spellLevel, BlueprintAbility spell)
	{
	}

	public override void OnEnable()
	{
	}
}
