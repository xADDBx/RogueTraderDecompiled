using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectRace : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public BlueprintRace Race;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Race;

	[JsonConstructor]
	public SelectRace()
	{
	}

	public SelectRace([NotNull] BlueprintRace race)
	{
		Race = race;
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
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Race);
		result.Append(ref val);
		return result;
	}
}
