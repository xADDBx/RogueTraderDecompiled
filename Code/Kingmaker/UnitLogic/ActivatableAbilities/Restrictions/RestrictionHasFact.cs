using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[TypeId("74999d4fdbb7037469dcc37ac14a67f5")]
public class RestrictionHasFact : ActivatableAbilityRestriction, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public bool Not;

	public BlueprintUnitFact Feature => m_Feature?.Get();

	protected override bool IsAvailable()
	{
		if (!Not)
		{
			return base.Owner.Facts.Contains(Feature);
		}
		return !base.Owner.Facts.Contains(Feature);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
