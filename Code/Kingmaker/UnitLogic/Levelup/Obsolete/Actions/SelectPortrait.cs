using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectPortrait : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintPortrait Portrait;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Visual;

	[JsonConstructor]
	public SelectPortrait()
	{
	}

	public SelectPortrait([NotNull] BlueprintPortrait portrait)
	{
		Portrait = portrait;
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
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Portrait);
		result.Append(ref val);
		return result;
	}
}
