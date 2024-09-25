using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class HandsEquipmentSet : IHashable
{
	[JsonProperty]
	public readonly HandSlot PrimaryHand;

	[JsonProperty]
	public readonly HandSlot SecondaryHand;

	public IEnumerable<HandSlot> Hands
	{
		get
		{
			yield return PrimaryHand;
			yield return SecondaryHand;
		}
	}

	public HandsEquipmentSet(BaseUnitEntity owner)
	{
		PrimaryHand = new HandSlot(owner);
		SecondaryHand = new HandSlot(owner);
	}

	[JsonConstructor]
	[UsedImplicitly]
	private HandsEquipmentSet()
	{
	}

	public bool IsEmpty()
	{
		if (!PrimaryHand.HasItem)
		{
			return !SecondaryHand.HasItem;
		}
		return false;
	}

	public void RetainDeactivateFlag()
	{
		PrimaryHand.RetainDeactivateFlag();
		SecondaryHand.RetainDeactivateFlag();
	}

	public void ReleaseDeactivateFlag()
	{
		PrimaryHand.ReleaseDeactivateFlag();
		SecondaryHand.ReleaseDeactivateFlag();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<HandSlot>.GetHash128(PrimaryHand);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<HandSlot>.GetHash128(SecondaryHand);
		result.Append(ref val2);
		return result;
	}
}
