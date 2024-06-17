using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectName : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public readonly string Name;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Visual;

	[JsonConstructor]
	public SelectName()
	{
	}

	public SelectName([NotNull] string name)
	{
		Name = name;
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
		result.Append(Name);
		return result;
	}
}
