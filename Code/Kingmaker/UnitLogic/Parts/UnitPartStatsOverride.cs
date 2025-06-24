using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartStatsOverride : UnitPart, IHashable
{
	[Serializable]
	private struct StatOverride : IHashable
	{
		public int value;

		public StatType statType;

		public Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	[JsonProperty]
	private List<StatOverride> overrides = new List<StatOverride>();

	public void AddOverride(StatType type, int value)
	{
		if (HasOverride(type))
		{
			RemoveOverride(type);
		}
		overrides.Add(new StatOverride
		{
			value = value,
			statType = type
		});
		base.Owner.Stats.GetStatOptional(type)?.UpdateValue();
	}

	public bool TryGetOverride(StatType type, out int value)
	{
		if (overrides.Any((StatOverride o) => o.statType == type))
		{
			value = overrides.First((StatOverride o) => o.statType == type).value;
			return true;
		}
		value = 0;
		return false;
	}

	public void Setup(List<StatType> stats, MechanicEntity entityToCopyFrom)
	{
		overrides = new List<StatOverride>();
		if (stats.Empty())
		{
			RemoveSelf();
		}
		foreach (StatType stat in stats)
		{
			AddOverride(stat, entityToCopyFrom.GetStatOptional(stat)?.ModifiedValue ?? 0);
		}
	}

	private void RemoveOverride(StatType type)
	{
		overrides.RemoveAt(overrides.FindIndex((StatOverride o) => o.statType == type));
		base.Owner.Stats.GetStatOptional(type)?.UpdateValue();
	}

	public void Remove()
	{
		List<StatType> list = overrides.Select((StatOverride o) => o.statType).ToList();
		while (list.Count > 0)
		{
			RemoveOverride(list[0]);
			list.RemoveAt(0);
		}
		RemoveSelf();
	}

	private bool HasOverride(StatType type)
	{
		return overrides.Any((StatOverride o) => o.statType == type);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<StatOverride> list = overrides;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				StatOverride obj = list[i];
				Hash128 val2 = UnmanagedHasher<StatOverride>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
