using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectRaceStat : ILevelUpAction, IHashable
{
	[JsonProperty]
	public readonly StatType Attribute;

	public LevelUpActionPriority Priority => LevelUpActionPriority.RaceStat;

	[JsonConstructor]
	public SelectRaceStat()
	{
	}

	public SelectRaceStat(StatType attribute)
	{
		Attribute = attribute;
	}

	public bool Check(LevelUpState state, BaseUnitEntity unit)
	{
		return false;
	}

	public void Apply(LevelUpState state, BaseUnitEntity unit)
	{
	}

	public void PostLoad()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		StatType val = Attribute;
		result.Append(ref val);
		return result;
	}
}
