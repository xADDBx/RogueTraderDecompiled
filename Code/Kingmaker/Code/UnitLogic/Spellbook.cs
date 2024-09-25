using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.UnitLogic;

public class Spellbook : IDisposable
{
	[CanBeNull]
	public readonly BlueprintSpellbook Blueprint;

	[CanBeNull]
	public readonly BaseUnitEntity Owner;

	public int CasterLevel => 0;

	public bool IsMythic => false;

	public int MaxSpellLevel => 0;

	public int LastSpellbookLevel => 0;

	public int FirstSpellbookLevel => 0;

	[JsonConstructor]
	public Spellbook(BaseUnitEntity owner, BlueprintSpellbook blueprint)
	{
	}

	[CanBeNull]
	public AbilityData AddKnown(int spellLevel, [NotNull] BlueprintAbility spell, bool raiseLearnSpellEvent = true)
	{
		return null;
	}

	public bool IsKnown([NotNull] BlueprintAbility spell)
	{
		return false;
	}

	public bool IsKnown([CanBeNull] AbilityData spell)
	{
		return false;
	}

	public void RemoveSpell(BlueprintAbility spell)
	{
	}

	public void AddSpecialList(BlueprintSpellList spellList)
	{
	}

	public bool CanRestoreSpells()
	{
		return false;
	}

	public void SwapSlots([NotNull] SpellSlot s1, [NotNull] SpellSlot s2)
	{
	}

	public bool Memorize([NotNull] AbilityData data, [CanBeNull] SpellSlot slot = null)
	{
		return false;
	}

	public bool IsMemorized([CanBeNull] AbilityData spell)
	{
		return false;
	}

	public int CalcSlotsLimit(int spellLevel, SpellSlotType slotType)
	{
		return 0;
	}

	public int CalcSlotsCost([NotNull] BlueprintAbility spell)
	{
		return 0;
	}

	public void ForgetMemorized([NotNull] SpellSlot spell)
	{
	}

	public int GetSpellsPerDay(int spellLevel)
	{
		return 0;
	}

	public int GetSpontaneousSlots(int level)
	{
		return 0;
	}

	public int GetSpellLevel([CanBeNull] BlueprintAbility spell)
	{
		return 0;
	}

	public int GetSpellLevel([NotNull] AbilityData data)
	{
		return 0;
	}

	[NotNull]
	public List<AbilityData> GetKnownSpells(int spellLevel)
	{
		return TempList.Get<AbilityData>();
	}

	public IEnumerable<AbilityData> GetAllKnownSpells()
	{
		return Enumerable.Empty<AbilityData>();
	}

	[NotNull]
	public IEnumerable<AbilityData> GetSpecialSpells(int spellLevel)
	{
		return Enumerable.Empty<AbilityData>();
	}

	[NotNull]
	public IEnumerable<AbilityData> GetCustomSpells(int spellLevel)
	{
		return Enumerable.Empty<AbilityData>();
	}

	public void AddCustomSpell(AbilityData abilityData)
	{
	}

	public void RemoveCustomSpell(AbilityData abilityData)
	{
	}

	[NotNull]
	public IEnumerable<SpellSlot> GetMemorizedSpellSlots(int spellLevel)
	{
		return Enumerable.Empty<SpellSlot>();
	}

	[NotNull]
	public IEnumerable<SpellSlot> GetMemorizedSpells(int spellLevel)
	{
		return Enumerable.Empty<SpellSlot>();
	}

	[NotNull]
	public IEnumerable<SpellSlot> GetAllMemorizedSpells()
	{
		return Enumerable.Empty<SpellSlot>();
	}

	public int GetConcentration()
	{
		return 0;
	}

	public void UpdateAllSlotsSize(bool allowShrink = false)
	{
	}

	public IEnumerable<BlueprintAbility> GetSpontaneousConversionSpells(int spellLevel)
	{
		return Enumerable.Empty<BlueprintAbility>();
	}

	public float GetFillFactor()
	{
		return 0f;
	}

	public void Dispose()
	{
	}

	public int GetTotalFreeSlotsCount()
	{
		return 0;
	}

	public bool IsSpellSpecial(AbilityData spellData)
	{
		return false;
	}

	public bool IsSpellCommon(AbilityData spellData)
	{
		return false;
	}

	public bool HasEnoughCasterLevel(int currentLevel)
	{
		return false;
	}

	public bool HasEnoughCastingStat(int currentLevel)
	{
		return false;
	}
}
