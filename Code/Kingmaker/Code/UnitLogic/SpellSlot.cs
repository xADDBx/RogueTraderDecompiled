using System;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic;

public class SpellSlot : IDisposable, IComparable<SpellSlot>, IHashable
{
	[JsonProperty]
	public readonly int SpellLevel;

	[JsonProperty]
	public readonly SpellSlotType Type;

	[JsonProperty]
	public readonly int Index;

	[CanBeNull]
	[JsonProperty]
	public AbilityData Spell;

	[JsonProperty]
	public bool Available;

	[CanBeNull]
	[JsonProperty]
	public SpellSlot[] LinkedSlots;

	[JsonProperty]
	public bool IsOpposition;

	public bool IsDouble
	{
		get
		{
			if (LinkedSlots != null)
			{
				return LinkedSlots.Length > 1;
			}
			return false;
		}
	}

	public int BusySlotsCount
	{
		get
		{
			SpellSlot[] linkedSlots = LinkedSlots;
			if (linkedSlots == null)
			{
				return 0;
			}
			return linkedSlots.Length;
		}
	}

	public bool IsMainSlot
	{
		get
		{
			if (LinkedSlots != null && LinkedSlots.Length != 0)
			{
				return LinkedSlots[0] == this;
			}
			return true;
		}
	}

	public SpellSlot(int spellLevel, SpellSlotType type, int index)
	{
		SpellLevel = spellLevel;
		Type = type;
		Index = index;
		Spell = null;
		LinkedSlots = null;
		Available = false;
	}

	[JsonConstructor]
	public SpellSlot()
	{
	}

	public void Dispose()
	{
	}

	public int CompareTo(SpellSlot other)
	{
		if (Type != other.Type)
		{
			return Type.CompareTo(other.Type);
		}
		int index = Index;
		return index.CompareTo(other.Index);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		int val = SpellLevel;
		result.Append(ref val);
		SpellSlotType val2 = Type;
		result.Append(ref val2);
		int val3 = Index;
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<AbilityData>.GetHash128(Spell);
		result.Append(ref val4);
		result.Append(ref Available);
		SpellSlot[] linkedSlots = LinkedSlots;
		if (linkedSlots != null)
		{
			for (int i = 0; i < linkedSlots.Length; i++)
			{
				Hash128 val5 = ClassHasher<SpellSlot>.GetHash128(linkedSlots[i]);
				result.Append(ref val5);
			}
		}
		result.Append(ref IsOpposition);
		return result;
	}
}
