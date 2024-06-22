using System;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.Settings;
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
}
