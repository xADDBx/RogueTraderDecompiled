using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[AllowedOn(typeof(BlueprintCareerPath))]
[TypeId("d8ced9f3bf5d40678c9458e43796e814")]
public class CareerPathUIMetaData : BlueprintComponent
{
	[SerializeField]
	[ItemNotNull]
	private BlueprintAbilityReference[] m_KeystoneAbilities = new BlueprintAbilityReference[0];

	[SerializeField]
	[ItemNotNull]
	private BlueprintFeatureReference[] m_KeystoneFeatures = new BlueprintFeatureReference[0];

	[SerializeField]
	[ItemNotNull]
	private BlueprintFeatureReference[] m_UltimateFeatures = new BlueprintFeatureReference[0];

	[SerializeField]
	[ItemNotNull]
	private BlueprintFeatureReference[] m_RecommendedFeatures = new BlueprintFeatureReference[0];

	[SerializeField]
	[ItemNotNull]
	private BlueprintFeatureReference[] m_RecommendedByOccupations = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintAbility> KeystoneAbilities
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] keystoneAbilities = m_KeystoneAbilities;
			return keystoneAbilities;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> KeystoneFeatures
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] keystoneFeatures = m_KeystoneFeatures;
			return keystoneFeatures;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> UltimateFeatures
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] ultimateFeatures = m_UltimateFeatures;
			return ultimateFeatures;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> RecommendedFeatures
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] recommendedFeatures = m_RecommendedFeatures;
			return recommendedFeatures;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> RecommendedByOccupations
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] recommendedByOccupations = m_RecommendedByOccupations;
			return recommendedByOccupations;
		}
	}
}
