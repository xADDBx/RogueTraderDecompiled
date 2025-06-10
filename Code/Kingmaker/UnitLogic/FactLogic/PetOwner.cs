using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("7e1a7a311da94107ad422fc99acf47ec")]
public class PetOwner : UnitFactComponentDelegate, IHashable
{
	[JsonProperty]
	public BlueprintPet.PetReference Pet;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartPetOwner>().Setup(Pet);
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			base.Owner.Remove<UnitPartPetOwner>();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(Pet);
		result.Append(ref val2);
		return result;
	}
}
