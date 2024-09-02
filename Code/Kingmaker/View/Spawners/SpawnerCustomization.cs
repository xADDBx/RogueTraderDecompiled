using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.Spawners;

[Obsolete("This customization doesn't match to in game logic. Use spawner's field: SelectedCustomizationVariation. Don't remove since some scenes may still contain this component")]
[RequireComponent(typeof(UnitSpawner))]
[KnowledgeDatabaseID("677166680e3e4bf3b2d222cb5d566200")]
public class SpawnerCustomization : MonoBehaviour, ICustomizationRequirements
{
	[CanBeNull]
	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("Preset")]
	private UnitCustomizationPresetReference m_Preset;

	[CanBeNull]
	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public UnitViewLink SelectedPrefab;

	[SerializeField]
	[FormerlySerializedAs("SelectedRace")]
	private BlueprintRaceReference m_SelectedRace;

	public Gender SelectedGender;

	[SerializeField]
	[FormerlySerializedAs("SelectedVoice")]
	private BlueprintUnitAsksListReference m_SelectedVoice;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("RaceRequirement")]
	private BlueprintRaceReference m_RaceRequirement;

	public bool HasGenderRequirement;

	[ConditionalShow("HasGenderRequirement")]
	public Gender GenderRequirement;

	public UnitCustomizationPreset Preset
	{
		get
		{
			return m_Preset?.Get();
		}
		set
		{
			m_Preset = value.ToReference<UnitCustomizationPresetReference>();
		}
	}

	public BlueprintUnit Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	public BlueprintRace SelectedRace
	{
		get
		{
			return m_SelectedRace?.Get();
		}
		set
		{
			m_SelectedRace = value.ToReference<BlueprintRaceReference>();
		}
	}

	public BlueprintUnitAsksList SelectedVoice
	{
		get
		{
			return m_SelectedVoice?.Get();
		}
		set
		{
			m_SelectedVoice = value.ToReference<BlueprintUnitAsksListReference>();
		}
	}

	public BlueprintRace RaceRequirement => m_RaceRequirement?.Get();

	public bool HasRequirements()
	{
		if (!HasGenderRequirement)
		{
			return RaceRequirement != null;
		}
		return true;
	}

	public bool FitsRequirements(UnitCustomizationVariation variation)
	{
		if (RaceRequirement != null && variation.Race != RaceRequirement)
		{
			return false;
		}
		if (HasGenderRequirement && variation.Gender != GenderRequirement)
		{
			return false;
		}
		return true;
	}

	public SpawningData RequestSpawningData()
	{
		return ContextData<SpawningData>.Request().Setup(SelectedPrefab.AssetId, SelectedRace, SelectedGender, SelectedVoice);
	}

	public void OnSpawn(BaseUnitEntity unit)
	{
	}
}
