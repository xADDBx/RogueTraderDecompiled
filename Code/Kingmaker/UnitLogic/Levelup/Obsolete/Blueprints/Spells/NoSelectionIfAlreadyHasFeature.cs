using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[AllowedOn(typeof(BlueprintFeatureSelection_Obsolete))]
[TypeId("7e9c938c232d48a4eb8458e448cfff33")]
public class NoSelectionIfAlreadyHasFeature : BlueprintComponent
{
	public bool AnyFeatureFromSelection;

	[SerializeField]
	[FormerlySerializedAs("Features")]
	private BlueprintFeatureReference[] m_Features;

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}
}
