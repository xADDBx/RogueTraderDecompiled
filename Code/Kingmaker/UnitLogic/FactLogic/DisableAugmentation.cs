using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dd0e4948aa064902adc1056084ae1f92")]
public class DisableAugmentation : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Body.Augments?.Disabled.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Body.Augments?.Disabled.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
