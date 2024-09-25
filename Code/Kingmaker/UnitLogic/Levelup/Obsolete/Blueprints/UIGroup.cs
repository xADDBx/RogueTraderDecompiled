using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Serializable]
public class UIGroup
{
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
}
