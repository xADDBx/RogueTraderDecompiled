using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[Serializable]
public class SelectionEntry
{
	[SerializeField]
	private BlueprintFeatureSelectionReference m_Selection;

	[SerializeField]
	private BlueprintFeatureReference[] m_Features;

	public BlueprintFeatureSelection_Obsolete Selection => m_Selection?.Get();

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}
}
