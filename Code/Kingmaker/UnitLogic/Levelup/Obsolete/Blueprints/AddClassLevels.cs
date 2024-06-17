using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("81d1333a815e48c4baf215e1b7adf8d6")]
public class AddClassLevels : UnitFactComponentDelegate, IHashable
{
	public class DoNotCreatePlan : ContextData<DoNotCreatePlan>
	{
		protected override void Reset()
		{
		}
	}

	public class ExecutionMark : ContextData<ExecutionMark>
	{
		protected override void Reset()
		{
		}
	}

	[ValidateNotNull]
	[SerializeField]
	private BlueprintCharacterClassReference m_CharacterClass;

	[ValidateNoNullEntries]
	[SerializeField]
	private BlueprintArchetypeReference[] m_Archetypes = new BlueprintArchetypeReference[0];

	public int Levels;

	public StatType RaceStat = StatType.WarhammerToughness;

	public StatType LevelsStat;

	public StatType[] Skills = new StatType[0];

	[ValidateNoNullEntries]
	[SerializeField]
	private BlueprintAbilityReference[] m_SelectSpells = new BlueprintAbilityReference[0];

	[ValidateNoNullEntries]
	[SerializeField]
	private BlueprintAbilityReference[] m_MemorizeSpells = new BlueprintAbilityReference[0];

	[ValidateNoNullEntries]
	public SelectionEntry[] Selections = new SelectionEntry[0];

	public bool DoNotApplyAutomatically;

	public BlueprintCharacterClass CharacterClass
	{
		get
		{
			return m_CharacterClass?.Get();
		}
		set
		{
			m_CharacterClass = value.ToReference<BlueprintCharacterClassReference>();
		}
	}

	public ReferenceArrayProxy<BlueprintArchetype> Archetypes
	{
		get
		{
			BlueprintReference<BlueprintArchetype>[] archetypes = m_Archetypes;
			return archetypes;
		}
	}

	public ReferenceArrayProxy<BlueprintAbility> SelectSpells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] selectSpells = m_SelectSpells;
			return selectSpells;
		}
	}

	public ReferenceArrayProxy<BlueprintAbility> MemorizeSpells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] memorizeSpells = m_MemorizeSpells;
			return memorizeSpells;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
