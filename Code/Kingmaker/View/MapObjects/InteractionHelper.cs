using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.Settings;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public static class InteractionHelper
{
	[Cheat(Name = "add_melta_charge", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddMeltaCharge(int count)
	{
		Game.Instance.Player.Inventory.Add(Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem, count);
	}

	[Cheat(Name = "add_item", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddItem([NotNull] string blueprintName, int count = 1)
	{
		BlueprintItem blueprint = Utilities.GetBlueprint<BlueprintItem>(blueprintName);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find item blueprint by name: {0}", blueprintName);
		}
		else
		{
			Game.Instance.Player.Inventory.Add(blueprint, count);
		}
	}

	public static InteractionActorType ToInteractionActorType(this StatType statType)
	{
		return statType switch
		{
			StatType.SkillLogic => InteractionActorType.Logic, 
			StatType.SkillLoreXenos => InteractionActorType.LoreXenos, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static int? GetUnitsToInteractCount(InteractionPart part)
	{
		if (part == null)
		{
			return null;
		}
		if (part.UnitsCanInteract == null)
		{
			return null;
		}
		int num = 0;
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			if (part.UnitsCanInteract.Contains(selectedUnit))
			{
				num++;
			}
		}
		return num;
	}

	public static void MarkUnitAsInteracted(BaseUnitEntity unit, InteractionPart part)
	{
		part.UnitsCanInteract?.Remove(unit);
	}

	public static int GetInteractionSkillCheckChance(BaseUnitEntity unit, StatType skill, int dcChance = 0)
	{
		int num = 0;
		if (dcChance != 0)
		{
			num = dcChance;
		}
		num += (int)SettingsRoot.Difficulty.SkillCheckModifier;
		if (unit != null)
		{
			ModifiableValue stat = unit.Stats.GetStat(skill);
			num += stat.ModifiedValue;
		}
		return Mathf.Clamp(num, 0, 100);
	}

	public static int GetNextRandomIdx(int count, bool doNotRepeatLastOne, ref int lastIdx)
	{
		if (count > 0)
		{
			if (count == 1)
			{
				lastIdx = 0;
				return lastIdx;
			}
			if (!doNotRepeatLastOne)
			{
				lastIdx = UnityEngine.Random.Range(0, count);
				return lastIdx;
			}
			int num;
			if (count == 2)
			{
				num = ((lastIdx < 0) ? UnityEngine.Random.Range(0, 2) : ((lastIdx == 0) ? 1 : 0));
			}
			else
			{
				List<int> list = Enumerable.Range(0, count).ToList();
				list.Remove(lastIdx);
				num = list[UnityEngine.Random.Range(0, list.Count)];
			}
			lastIdx = num;
			return num;
		}
		lastIdx = -1;
		return lastIdx;
	}
}
