using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectSpell : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintSpellbook Spellbook;

	[NotNull]
	[JsonProperty]
	public readonly BlueprintSpellList SpellList;

	[JsonProperty]
	public readonly int SpellLevel;

	[NotNull]
	[JsonProperty]
	public readonly BlueprintAbility Spell;

	[JsonProperty]
	public readonly int SlotIndex;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Spells;

	[JsonConstructor]
	public SelectSpell()
	{
	}

	public SelectSpell([NotNull] BlueprintSpellbook spellbook, [NotNull] BlueprintSpellList spellList, int spellLevel, [NotNull] BlueprintAbility spell, int slotIndex)
	{
		Spellbook = spellbook;
		SpellList = spellList;
		SpellLevel = spellLevel;
		Spell = spell;
		SlotIndex = slotIndex;
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
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Spellbook);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(SpellList);
		result.Append(ref val2);
		int val3 = SpellLevel;
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Spell);
		result.Append(ref val4);
		int val5 = SlotIndex;
		result.Append(ref val5);
		return result;
	}
}
