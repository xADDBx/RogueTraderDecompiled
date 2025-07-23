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
	[JsonProperty(PropertyName = "PrimaryHand")]
	private HandSlot m_PrimaryHand;

	[JsonProperty(PropertyName = "SecondaryHand")]
	private HandSlot m_SecondaryHand;

	private HandSlot m_OverridePrimaryHand;

	private HandSlot m_OverrideSecondaryHand;

	[JsonIgnore]
	public HandSlot PrimaryHand => m_OverridePrimaryHand ?? m_PrimaryHand;

	[JsonIgnore]
	public HandSlot SecondaryHand => m_OverrideSecondaryHand ?? m_SecondaryHand;

	[JsonIgnore]
	public bool IsOverridePrimaryHand => m_OverridePrimaryHand != null;

	[JsonIgnore]
	public bool IsOverrideSecondaryHand => m_OverrideSecondaryHand != null;

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
		m_PrimaryHand = new HandSlot(owner);
		m_SecondaryHand = new HandSlot(owner);
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

	public void OverridePrimaryHand(HandSlot primaryHand)
	{
		m_OverridePrimaryHand = primaryHand;
	}

	public void OverrideSecondaryHand(HandSlot secondaryHand)
	{
		m_OverrideSecondaryHand = secondaryHand;
	}

	public bool HasSlot(HandSlot handSlot)
	{
		if (m_PrimaryHand != handSlot)
		{
			return m_SecondaryHand == handSlot;
		}
		return true;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<HandSlot>.GetHash128(m_PrimaryHand);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<HandSlot>.GetHash128(m_SecondaryHand);
		result.Append(ref val2);
		return result;
	}
}
