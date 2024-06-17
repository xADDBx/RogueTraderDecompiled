using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Progression;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class ProgressionRoot
{
	[NotNull]
	[SerializeField]
	private List<BlueprintCareerPath.Reference> m_CareerPaths;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("FeatsProgression")]
	private BlueprintProgressionReference m_FeatsProgression;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharacterClasses")]
	private BlueprintCharacterClassReference[] m_CharacterClasses;

	[NotNull]
	[SerializeField]
	private BlueprintCharacterClassReference m_MythicStartingClass;

	[NotNull]
	[SerializeField]
	private BlueprintCharacterClassReference m_MythicCompanionClass;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharacterMythics")]
	private BlueprintCharacterClassReference[] m_CharacterMythics;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("AnimalCompanion")]
	private BlueprintCharacterClassReference m_AnimalCompanion;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharacterRaces")]
	private BlueprintRaceReference[] m_CharacterRaces;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CharGenMemorizeSpells")]
	private BlueprintAbilityReference[] m_CharGenMemorizeSpells;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("XPTable")]
	private BlueprintStatProgressionReference m_XPTable;

	[NotNull]
	[SerializeField]
	private BlueprintStatProgressionReference m_LegendXPTable;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CRTable")]
	private BlueprintStatProgressionReference m_CRTable;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("XPToCRTable")]
	private BlueprintStatProgressionReference m_XPToCRTable;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("DCToCRTable")]
	private BlueprintStatProgressionReference m_DCToCRTable;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("CategoryDefaults")]
	private BlueprintCategoryDefaultsReference m_CategoryDefaults;

	[SerializeField]
	public StatProgressions StatProgressions;

	[SerializeField]
	public float SummonedUnitExperienceFactor = 0.1f;

	[Header("Starship Progression")]
	[NotNull]
	[SerializeField]
	private BlueprintStatProgressionReference m_StarshipXPTable;

	[NotNull]
	[SerializeField]
	private BlueprintAbilitiesUnlockTableReference m_AbilitiesUnlockTable;

	[NotNull]
	[SerializeField]
	private BlueprintShipComponentsUnlockTableReference m_StarshipComponentsUnlockTable;

	[NotNull]
	[SerializeField]
	private BlueprintCareerPath.Reference m_ShipPath;

	[NotNull]
	[SerializeField]
	private BlueprintShipSystemComponent.Reference m_ShipSystemComponents;

	public static ProgressionRoot Instance => BlueprintRoot.Instance.Progression;

	public IEnumerable<BlueprintCareerPath> CareerPaths => m_CareerPaths.EmptyIfNull().Dereference();

	public BlueprintProgression FeatsProgression => m_FeatsProgression?.Get();

	public ReferenceArrayProxy<BlueprintCharacterClass> CharacterClasses
	{
		get
		{
			BlueprintReference<BlueprintCharacterClass>[] characterClasses = m_CharacterClasses;
			return characterClasses;
		}
	}

	public BlueprintCharacterClass MythicStartingClass => m_MythicStartingClass?.Get();

	public BlueprintCharacterClass MythicCompanionClass => m_MythicCompanionClass?.Get();

	public ReferenceArrayProxy<BlueprintCharacterClass> CharacterMythics
	{
		get
		{
			BlueprintReference<BlueprintCharacterClass>[] characterMythics = m_CharacterMythics;
			return characterMythics;
		}
	}

	public BlueprintCharacterClass AnimalCompanion => m_AnimalCompanion?.Get();

	public ReferenceArrayProxy<BlueprintRace> CharacterRaces
	{
		get
		{
			BlueprintReference<BlueprintRace>[] characterRaces = m_CharacterRaces;
			return characterRaces;
		}
	}

	public ReferenceArrayProxy<BlueprintAbility> CharGenMemorizeSpells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] charGenMemorizeSpells = m_CharGenMemorizeSpells;
			return charGenMemorizeSpells;
		}
	}

	public BlueprintStatProgression XPTable => m_XPTable?.Get();

	public BlueprintStatProgression LegendXPTable => m_LegendXPTable?.Get();

	public BlueprintStatProgression CRTable => m_CRTable?.Get();

	public BlueprintStatProgression XPToCRTable => m_XPToCRTable?.Get();

	public BlueprintStatProgression DCToCRTable => m_DCToCRTable?.Get();

	public BlueprintCategoryDefaults CategoryDefaults => m_CategoryDefaults?.Get();

	public BlueprintStatProgression StarshipXPTable => m_StarshipXPTable?.Get();

	public BlueprintAbilitiesUnlockTable AbilitiesUnlockTable => m_AbilitiesUnlockTable?.Get();

	public BlueprintShipComponentsUnlockTable StarshipComponentsUnlockTable => m_StarshipComponentsUnlockTable?.Get();

	public BlueprintCareerPath ShipPath => m_ShipPath?.Get();

	public BlueprintShipSystemComponent ShipSystemComponents => m_ShipSystemComponents?.Get();
}
