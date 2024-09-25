using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectAlignment : ILevelUpAction, IHashable
{
	[JsonProperty]
	public readonly Alignment Alignment;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Alignment;

	[JsonConstructor]
	public SelectAlignment()
	{
	}

	public SelectAlignment(Alignment alignment)
	{
		Alignment = alignment;
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
		Alignment val = Alignment;
		result.Append(ref val);
		return result;
	}
}
