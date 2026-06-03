using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("8d44f1b6309148a3a50778e4bc3ff784")]
public class ContextActionUseItemFromInventory : ContextAction
{
	private enum ItemSelectionMethod
	{
		First,
		Random
	}

	[SerializeField]
	private ItemSelectionMethod m_SelectionMethod;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	[Tooltip("Enables animation for casting and initiates full UseAbility command instead of simply triggering cast rule")]
	[SerializeField]
	private bool m_UseFullAbilityCastCycle;

	public override string GetCaption()
	{
		return $"Uses {m_SelectionMethod} item directly from Caster's inventory";
	}

	protected override void RunAction()
	{
		if (!(base.Caster is BaseUnitEntity))
		{
			PFLog.Actions.Log(this, "Caster is not a unit");
			return;
		}
		AbilityData abilityData = FindAbility();
		if (abilityData == null)
		{
			PFLog.Actions.Log(this, "No suitable usable item found for this target");
			return;
		}
		if (m_UseFullAbilityCastCycle)
		{
			PartUnitCommands commandsOptional = base.Caster.GetCommandsOptional();
			if (commandsOptional != null)
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(abilityData, base.Target)
				{
					IgnoreCooldown = true,
					FreeAction = true
				};
				commandsOptional.AddToQueue(cmdParams);
				return;
			}
		}
		RulePerformAbility obj = new RulePerformAbility(abilityData, base.Target)
		{
			IgnoreCooldown = true,
			ForceFreeAction = true
		};
		Rulebook.Trigger(obj);
		obj.Context.RewindActionIndex();
		if (obj.Result != null)
		{
			abilityData.RollAndTrySpend();
		}
	}

	private AbilityData FindAbility()
	{
		List<AbilityData> list = FindSuitableAbilities().ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return SelectAbilityFromAvailable(list);
	}

	private AbilityData SelectAbilityFromAvailable(IReadOnlyList<AbilityData> suitableItems)
	{
		return m_SelectionMethod switch
		{
			ItemSelectionMethod.First => suitableItems[0], 
			ItemSelectionMethod.Random => suitableItems.Random(((AbstractUnitEntity)base.Caster).Random), 
			_ => throw new NotImplementedException($"Unsupported selection method: {m_SelectionMethod}"), 
		};
	}

	private IEnumerable<AbilityData> FindSuitableAbilities()
	{
		PartInventory inventoryOptional = base.Caster.GetInventoryOptional();
		if (inventoryOptional == null)
		{
			yield break;
		}
		foreach (ItemEntityUsable item in inventoryOptional.Collection.Items.OfType<ItemEntityUsable>())
		{
			BlueprintAbility blueprintAbility = item.Blueprint.Abilities?.FirstOrDefault();
			if (blueprintAbility != null && IsSuitableAbility(blueprintAbility))
			{
				AbilityData abilityData = GetAbilityData(item, blueprintAbility);
				if (abilityData.IsAvailableForForcedUse && abilityData.CanTarget(base.Target))
				{
					yield return abilityData;
				}
			}
		}
	}

	private AbilityData GetAbilityData(ItemEntity item, BlueprintAbility abilityBp)
	{
		return ((BaseUnitEntity)base.Caster).Abilities.Enumerable.Find((Ability fact) => fact.SourceItem == item && fact.Blueprint == abilityBp)?.Data ?? new AbilityData(abilityBp, base.Caster)
		{
			OverrideSourceItem = item
		};
	}

	private bool IsSuitableAbility(BlueprintAbility abilityBp)
	{
		if (m_AbilityGroups == null || m_AbilityGroups.Length == 0)
		{
			return true;
		}
		return m_AbilityGroups.Any((BlueprintAbilityGroupReference gr) => abilityBp.AbilityGroups.Contains(gr));
	}
}
