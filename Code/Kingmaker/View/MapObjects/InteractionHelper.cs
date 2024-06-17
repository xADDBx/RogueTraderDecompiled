using System;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;

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
}
