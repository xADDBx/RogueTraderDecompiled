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
public class SelectClass : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	private readonly BlueprintCharacterClass m_CharacterClass;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Class;

	public BlueprintCharacterClass CharacterClass => m_CharacterClass;

	[JsonConstructor]
	public SelectClass()
	{
	}

	public SelectClass(BlueprintCharacterClass characterClass)
	{
		m_CharacterClass = characterClass;
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
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_CharacterClass);
		result.Append(ref val);
		return result;
	}
}
