using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("b27e65e0440f49409a5f19da2483cd1e")]
public class AddPsykerPart : UnitFactComponentDelegate, IHashable
{
	public int PsyRating;

	protected override void OnActivateOrPostLoad()
	{
		PartPsyker orCreate = base.Owner.Parts.GetOrCreate<PartPsyker>();
		orCreate.Retain();
		orCreate.PsyRating.AddModifier(PsyRating, base.Runtime, ModifierDescriptor.UntypedUnstackable);
	}

	protected override void OnDeactivate()
	{
		PartPsyker orCreate = base.Owner.Parts.GetOrCreate<PartPsyker>();
		orCreate.PsyRating.RemoveModifiersFrom(base.Runtime);
		orCreate.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
