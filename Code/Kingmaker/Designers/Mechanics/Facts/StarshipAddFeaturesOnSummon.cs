using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("6b4b35d97564c3a4f900256bf0200a09")]
public class StarshipAddFeaturesOnSummon : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private bool ReverseLogicApplyToAllExcept;

	[SerializeField]
	private BlueprintStarshipReference[] m_Blueprints = new BlueprintStarshipReference[0];

	[SerializeField]
	private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintStarship> Blueprints
	{
		get
		{
			BlueprintReference<BlueprintStarship>[] blueprints = m_Blueprints;
			return blueprints;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}

	public void ProcessUnit(BaseUnitEntity ship)
	{
		if (!(ReverseLogicApplyToAllExcept ^ Blueprints.Contains(ship.Blueprint)))
		{
			return;
		}
		foreach (BlueprintFeature feature in Features)
		{
			ship.AddFact(feature);
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
