using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintCharacterClass))]
[AllowedOn(typeof(BlueprintPath))]
[TypeId("00cc9fe0a3f84bc1811764509b4282aa")]
public class AddFeaturesToLevelUp : BlueprintComponent
{
	public FeatureGroup Group;

	[SerializeField]
	private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}

	public void Editor_AddFeature(BlueprintFeature feature)
	{
		Array.Resize(ref m_Features, m_Features.Length + 1);
		m_Features[^1] = feature.ToReference<BlueprintFeatureReference>();
	}
}
