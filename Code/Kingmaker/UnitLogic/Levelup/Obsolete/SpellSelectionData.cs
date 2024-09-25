using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

public class SpellSelectionData : IUIDataProvider
{
	public class SpellSelectionState
	{
		public BlueprintAbility[] SpellSelections;
	}

	[NotNull]
	public readonly BlueprintSpellbook Spellbook;

	[NotNull]
	public readonly BlueprintSpellList SpellList;

	public readonly SpellSelectionState[] LevelCount;

	public int ExtraMaxLevel = -1;

	public bool ExtraByStat;

	public BlueprintAbility[] ExtraSelected;

	public string Name => string.Empty;

	public string Description => string.Empty;

	public Sprite Icon => null;

	public string NameForAcronym => null;

	public SpellSelectionData(BlueprintSpellbook spellbook, BlueprintSpellList spellList)
	{
		Spellbook = spellbook;
		SpellList = spellList;
		LevelCount = new SpellSelectionState[11];
	}

	public SpellSelectionData(BlueprintSpellbook spellbook)
		: this(spellbook, spellbook.SpellList)
	{
	}

	public void SetExtraSpells(int count, int maxLevel)
	{
		if (ExtraMaxLevel >= 0 && maxLevel != ExtraMaxLevel)
		{
			PFLog.Default.Error("Trying to add extra spells more than once. It is not supported currently");
			return;
		}
		ExtraMaxLevel = maxLevel;
		ExtraSelected = new BlueprintAbility[count];
	}

	public void SetLevelSpells(int level, int count)
	{
		LevelCount[level] = new SpellSelectionState();
		LevelCount[level].SpellSelections = new BlueprintAbility[count];
	}

	public void SpendSlot(int spellLevel, BlueprintAbility spell, int slotIndex)
	{
		SpellSelectionState spellSelectionState = LevelCount[spellLevel];
		if (spellSelectionState != null && slotIndex >= 0 && slotIndex < spellSelectionState.SpellSelections.Length)
		{
			spellSelectionState.SpellSelections[slotIndex] = spell;
		}
		if (ExtraSelected != null && ExtraMaxLevel >= spellLevel && slotIndex >= 0 && slotIndex < ExtraSelected.Length)
		{
			ExtraSelected[slotIndex] = spell;
		}
	}

	public bool CanSpendSlot(int spellLevel, int slotIndex)
	{
		SpellSelectionState spellSelectionState = LevelCount[spellLevel];
		if (spellSelectionState != null && slotIndex >= 0 && slotIndex < spellSelectionState.SpellSelections.Length && spellSelectionState.SpellSelections[slotIndex] == null)
		{
			return true;
		}
		if (ExtraSelected != null && ExtraMaxLevel >= spellLevel && slotIndex >= 0 && slotIndex < ExtraSelected.Length && ExtraSelected[slotIndex] == null)
		{
			return true;
		}
		return false;
	}

	public bool HasEmpty()
	{
		if (ExtraSelected != null && ExtraSelected.Length != 0)
		{
			BlueprintAbility[] extraSelected = ExtraSelected;
			for (int i = 0; i < extraSelected.Length; i++)
			{
				if (extraSelected[i] == null)
				{
					return true;
				}
			}
		}
		for (int j = 0; j < LevelCount.Length; j++)
		{
			if (LevelCount[j] == null || LevelCount.Length == 0 || LevelCount[j].SpellSelections == null)
			{
				continue;
			}
			BlueprintAbility[] extraSelected = LevelCount[j].SpellSelections;
			for (int i = 0; i < extraSelected.Length; i++)
			{
				if (extraSelected[i] == null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanSelectAnything(BaseUnitEntity unit)
	{
		Spellbook spellbook = unit.Spellbooks.FirstOrDefault((Spellbook s) => s.Blueprint == Spellbook);
		if (spellbook == null)
		{
			return false;
		}
		if (ExtraSelected != null && ExtraSelected.Length != 0 && ExtraSelected.HasItem((BlueprintAbility i) => i == null))
		{
			for (int j = 0; j <= ExtraMaxLevel; j++)
			{
				int ii = j;
				if (SpellList.SpellsByLevel[j].SpellsFiltered.HasItem((BlueprintAbility sb) => !sb.IsCantrip && !SpellbookContainsSpell(spellbook, ii, sb)))
				{
					return true;
				}
			}
		}
		for (int k = 0; k < LevelCount.Length; k++)
		{
			if (LevelCount[k] != null && LevelCount.Length != 0 && LevelCount[k].SpellSelections != null && LevelCount[k].SpellSelections.HasItem((BlueprintAbility s) => s == null))
			{
				int ii = k;
				if (SpellList.SpellsByLevel[k].SpellsFiltered.HasItem((BlueprintAbility sb) => !SpellbookContainsSpell(spellbook, ii, sb)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool SpellbookContainsSpell(Spellbook spellbook, int ii, BlueprintAbility sb)
	{
		return spellbook.GetKnownSpells(ii).FirstOrDefault((AbilityData a) => a.Blueprint == sb) != null;
	}

	public void UpdateMaxLevelSpells(BaseUnitEntity unit)
	{
		if (ExtraByStat)
		{
			int num = unit.Stats.GetStat(Spellbook.CastingAttribute).PermanentValue / 2 - 5;
			int num2 = 3 + num;
			if (ExtraSelected.Length != num2 && num2 >= 0)
			{
				ExtraSelected = new BlueprintAbility[num2];
			}
		}
	}
}
