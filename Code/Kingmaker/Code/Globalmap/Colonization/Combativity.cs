using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class Combativity : IHashable
{
	[JsonProperty]
	public float InitialValue;

	[JsonProperty]
	public List<CombativityModifier> ModifiersTotal = new List<CombativityModifier>();

	[JsonProperty]
	public bool IsInitialized;

	private float ModifiersTotalValue => ModifiersTotal.Aggregate(InitialValue, (float sum, CombativityModifier modifier) => sum += modifier.Value);

	public float Total => Mathf.Max(0f, ModifiersTotalValue);

	public void AddModifier(float value, ProfitFactorModifierType type = ProfitFactorModifierType.Other, BlueprintScriptableObject modifier = null)
	{
		if (!(Mathf.Abs(value) < Mathf.Epsilon))
		{
			CombativityModifier mod = new CombativityModifier
			{
				Value = value,
				ModifierType = type,
				Modifier = modifier
			};
			ModifiersTotal.Add(mod);
			EventBus.RaiseEvent(delegate(ICombativityHandler h)
			{
				h.HandleCombativityModifierAdded(value, mod);
			});
		}
	}

	public void RemoveModifier(BlueprintScriptableObject modifier)
	{
		if (modifier == null)
		{
			return;
		}
		CombativityModifier m = ModifiersTotal.FirstOrDefault((CombativityModifier mod) => mod.Modifier == modifier);
		if (m != null)
		{
			ModifiersTotal.Remove(m);
			EventBus.RaiseEvent(delegate(ICombativityHandler h)
			{
				h.HandleCombativityModifierRemoved(m.Value, m);
			});
		}
	}

	public float? GetModifierValue(BlueprintScriptableObject modifier)
	{
		if (modifier == null)
		{
			return null;
		}
		return ModifiersTotal.FirstOrDefault((CombativityModifier mod) => mod.Modifier == modifier)?.Value;
	}

	public List<CombativityModifier> GetModifiersByType(ProfitFactorModifierType type)
	{
		return ModifiersTotal.Where((CombativityModifier modifier) => modifier.ModifierType == type)?.EmptyIfNull().ToList();
	}

	public void Initialize()
	{
		if (!IsInitialized)
		{
			InitialValue = BlueprintWarhammerRoot.Instance.CombativityRoot.InitialProfitFactor;
			IsInitialized = true;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref InitialValue);
		List<CombativityModifier> modifiersTotal = ModifiersTotal;
		if (modifiersTotal != null)
		{
			for (int i = 0; i < modifiersTotal.Count; i++)
			{
				Hash128 val = ClassHasher<CombativityModifier>.GetHash128(modifiersTotal[i]);
				result.Append(ref val);
			}
		}
		result.Append(ref IsInitialized);
		return result;
	}
}
