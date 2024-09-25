using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectGender : ILevelUpAction, IHashable
{
	[JsonProperty]
	public readonly Gender Gender;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Visual;

	[JsonConstructor]
	public SelectGender()
	{
	}

	public SelectGender(Gender gender)
	{
		Gender = gender;
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
		Gender val = Gender;
		result.Append(ref val);
		return result;
	}
}
