using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("f46c5f588edca9e47b0dc0509fbcf591")]
public class RemoveFeatureOnApply : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public BlueprintUnitFact Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		base.Owner.Facts.Remove(Feature);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
