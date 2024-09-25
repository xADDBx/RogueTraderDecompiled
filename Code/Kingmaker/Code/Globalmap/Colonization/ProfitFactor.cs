using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class ProfitFactor : IHashable
{
	[JsonProperty]
	public float InitialValue;

	[JsonProperty]
	public List<ProfitFactorModifier> ModifiersTotal = new List<ProfitFactorModifier>();

	[JsonProperty]
	public Dictionary<FactionType, int> VendorDiscounts = new Dictionary<FactionType, int>();

	[JsonProperty]
	public bool IsInitialized;

	private float ModifiersTotalValue => ModifiersTotal.Aggregate(InitialValue, (float sum, ProfitFactorModifier modifier) => sum += modifier.Value);

	public float Total => Mathf.Max(0f, ModifiersTotalValue);

	public void AddModifier(float value, ProfitFactorModifierType type = ProfitFactorModifierType.Other, BlueprintScriptableObject modifier = null)
	{
		if (!(Mathf.Abs(value) < Mathf.Epsilon))
		{
			ProfitFactorModifier mod = new ProfitFactorModifier
			{
				Value = value,
				ModifierType = type,
				Modifier = modifier
			};
			ModifiersTotal.Add(mod);
			EventBus.RaiseEvent(delegate(IProfitFactorHandler h)
			{
				h.HandleProfitFactorModifierAdded(value, mod);
			});
		}
	}

	public void RemoveModifier(BlueprintScriptableObject modifier)
	{
		if (modifier == null)
		{
			return;
		}
		ProfitFactorModifier m = ModifiersTotal.FirstOrDefault((ProfitFactorModifier mod) => mod.Modifier == modifier);
		if (m != null)
		{
			ModifiersTotal.Remove(m);
			EventBus.RaiseEvent(delegate(IProfitFactorHandler h)
			{
				h.HandleProfitFactorModifierRemoved(m.Value, m);
			});
		}
	}

	public float? GetModifierValue(BlueprintScriptableObject modifier)
	{
		if (modifier == null)
		{
			return null;
		}
		return ModifiersTotal.FirstOrDefault((ProfitFactorModifier mod) => mod.Modifier == modifier)?.Value;
	}

	public List<ProfitFactorModifier> GetModifiersByType(ProfitFactorModifierType type)
	{
		return ModifiersTotal.Where((ProfitFactorModifier modifier) => modifier.ModifierType == type)?.EmptyIfNull().ToList();
	}

	public void Initialize()
	{
		if (!IsInitialized)
		{
			InitialValue = BlueprintWarhammerRoot.Instance.ProfitFactorRoot.InitialProfitFactor;
			IsInitialized = true;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref InitialValue);
		List<ProfitFactorModifier> modifiersTotal = ModifiersTotal;
		if (modifiersTotal != null)
		{
			for (int i = 0; i < modifiersTotal.Count; i++)
			{
				Hash128 val = ClassHasher<ProfitFactorModifier>.GetHash128(modifiersTotal[i]);
				result.Append(ref val);
			}
		}
		Dictionary<FactionType, int> vendorDiscounts = VendorDiscounts;
		if (vendorDiscounts != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<FactionType, int> item in vendorDiscounts)
			{
				Hash128 hash = default(Hash128);
				FactionType obj = item.Key;
				Hash128 val3 = UnmanagedHasher<FactionType>.GetHash128(ref obj);
				hash.Append(ref val3);
				int obj2 = item.Value;
				Hash128 val4 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		result.Append(ref IsInitialized);
		return result;
	}
}
