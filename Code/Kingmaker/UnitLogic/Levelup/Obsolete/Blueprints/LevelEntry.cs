using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Serializable]
public class LevelEntry
{
	[NotNull]
	internal static LevelEntry Empty = new LevelEntry();

	public int Level;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Features")]
	private List<BlueprintFeatureBaseReference> m_Features = new List<BlueprintFeatureBaseReference>();

	private ReferenceListProxy<BlueprintFeatureBase, BlueprintFeatureBaseReference> m_FeaturesList;

	public IList<BlueprintFeatureBase> Features
	{
		get
		{
			ReferenceListProxy<BlueprintFeatureBase, BlueprintFeatureBaseReference> obj = m_FeaturesList ?? ((ReferenceListProxy<BlueprintFeatureBase, BlueprintFeatureBaseReference>)m_Features);
			ReferenceListProxy<BlueprintFeatureBase, BlueprintFeatureBaseReference> result = obj;
			m_FeaturesList = obj;
			return result;
		}
	}

	public void SetFeatures(IEnumerable<BlueprintFeatureBase> features)
	{
		m_Features = features.Select((BlueprintFeatureBase f) => f.ToReference<BlueprintFeatureBaseReference>()).ToList();
		m_FeaturesList = null;
	}
}
