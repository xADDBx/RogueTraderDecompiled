using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class AddArchetype : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintCharacterClass CharacterClass;

	[NotNull]
	[JsonProperty]
	public readonly BlueprintArchetype Archetype;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Archetype;

	[JsonConstructor]
	public AddArchetype()
	{
	}

	public AddArchetype([NotNull] BlueprintCharacterClass characterClass, [NotNull] BlueprintArchetype archetype)
	{
		CharacterClass = characterClass;
		Archetype = archetype;
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
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(CharacterClass);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Archetype);
		result.Append(ref val2);
		return result;
	}
}
