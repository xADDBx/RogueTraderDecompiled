using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class AdditionalLimbSlot : WeaponSlot, IHashable
{
	[JsonProperty]
	public bool KeepInPolymorph { get; set; }

	public AdditionalLimbSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public AdditionalLimbSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = KeepInPolymorph;
		result.Append(ref val2);
		return result;
	}
}
