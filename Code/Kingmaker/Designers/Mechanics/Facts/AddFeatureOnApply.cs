using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("0a53ee09075a237408f1347648c1e91a")]
public class AddFeatureOnApply : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		if (m_Restrictions.IsPassed(base.Fact))
		{
			base.Owner.Progression.Features.Add(Feature);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Progression.Features.Remove(Feature);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
