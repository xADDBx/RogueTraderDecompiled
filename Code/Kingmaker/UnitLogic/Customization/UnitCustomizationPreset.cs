using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[TypeId("55563e28cf984d0e8017b467ebfa4bba")]
public class UnitCustomizationPreset : BlueprintScriptableObject
{
	[HideInInspector]
	public List<PresetObject> PresetObjects = new List<PresetObject>();

	public int VariationsCount = 10;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Distribution")]
	private RaceGenderDistributionReference m_Distribution;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Units")]
	private BlueprintUnitReference[] m_Units = new BlueprintUnitReference[0];

	[NotNull]
	public ClothesSelection[] ClothesSelections = new ClothesSelection[0];

	[NotNull]
	public List<UnitVariations> UnitVariations = new List<UnitVariations>();

	[NotNull]
	public List<BlueprintUnitAsksListReference> MaleVoices = new List<BlueprintUnitAsksListReference>();

	[NotNull]
	public List<BlueprintUnitAsksListReference> FemaleVoices = new List<BlueprintUnitAsksListReference>();

	public RaceGenderDistribution Distribution => m_Distribution?.Get();

	public ReferenceArrayProxy<BlueprintUnit> Units
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] units = m_Units;
			return units;
		}
	}

	[CanBeNull]
	public UnitCustomizationVariation SelectVariation([NotNull] BlueprintUnit unit, [CanBeNull] ICustomizationRequirements requirements)
	{
		foreach (UnitVariations unitVariation in UnitVariations)
		{
			if (unitVariation.Units.Contains(unit))
			{
				List<UnitCustomizationVariation> list = unitVariation.Variations.Where((UnitCustomizationVariation v) => v != null && (requirements == null || requirements.FitsRequirements(v))).ToList();
				string id = $"{AssetGuid}_{unitVariation.Units.First((BlueprintUnit u) => u != null)?.AssetGuid}_{list.Count}";
				return PrimeRotation.SelectNext(list, id, PFStatefulRandom.UnitLogic.Customization);
			}
		}
		return null;
	}

	public BlueprintUnitAsksList SelectVoice(Gender gender)
	{
		List<BlueprintUnitAsksListReference> list = ((gender == Gender.Male) ? MaleVoices : FemaleVoices);
		string id = $"{AssetGuid}_{gender}_Voice";
		return PrimeRotation.SelectNext(list, id, PFStatefulRandom.UnitLogic.Customization)?.Get();
	}
}
