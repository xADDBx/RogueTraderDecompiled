using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("7c3693f332324ec4f94935a796c848f9")]
public class WarhammerOverrideGroupCooldown : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateGroupCooldown>, IRulebookHandler<RuleCalculateGroupCooldown>, ISubscriber, IInitiatorRulebookSubscriber, IEntitySubscriber, IUnitAbilityCooldownHandler<EntitySubscriber>, IUnitAbilityCooldownHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitAbilityCooldownHandler, EntitySubscriber>, IHashable
{
	public class ComponentData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int Charges = -1;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Charges);
			return result;
		}
	}

	private enum OverrideStrategyType
	{
		Override,
		Increase,
		Reduce
	}

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private OverrideStrategyType m_OverrideStrategy;

	[SerializeField]
	[ShowIf("m_IsOverrideStrategy")]
	private bool m_Infinite;

	[SerializeField]
	[HideIf("m_Infinite")]
	private int m_Value;

	public bool LimitedCharges;

	[ShowIf("LimitedCharges")]
	public ContextValue Charges = -1;

	public bool CostRestricted;

	[ShowIf("CostRestricted")]
	public int MaxActionPointsCost = -1;

	public bool RefundActionPointCost;

	public ActionList ActionsOnOverride;

	[ShowIf("LimitedCharges")]
	public ActionList ActionsAfterCharges;

	public bool ForbidOtherAbilities;

	[SerializeField]
	private BlueprintAbilityGroupReference m_FilterGroup;

	public bool OnlyChosenWeapon;

	public bool OnlyCheapestAbilities;

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	private bool m_IsOverrideStrategy => m_OverrideStrategy == OverrideStrategyType.Override;

	public BlueprintAbilityGroup FilterGroup => m_FilterGroup;

	protected override void OnActivate()
	{
		if (LimitedCharges)
		{
			int num = Charges.Calculate(base.Context);
			if (num > 0)
			{
				RequestSavableData<ComponentData>().Charges = num;
			}
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		if (!ForbidOtherAbilities)
		{
			return;
		}
		if (CostRestricted)
		{
			if (OnlyChosenWeapon)
			{
				ItemEntityWeapon weapon = base.Owner.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
				ForbidAffectedAbilityGroups(AffectedGroup, MaxActionPointsCost, base.Fact, weapon);
			}
			else
			{
				ForbidAffectedAbilityGroups(AffectedGroup, MaxActionPointsCost, base.Fact);
			}
		}
		else if (OnlyChosenWeapon)
		{
			ItemEntityWeapon weapon2 = base.Owner.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
			ForbidAffectedAbilityGroups(AffectedGroup, 10, base.Fact, weapon2);
		}
	}

	private void ForbidAffectedAbilityGroups(BlueprintAbilityGroup group, int lowerCostException, UnitFact reason, ItemEntityWeapon weapon = null)
	{
		foreach (BlueprintAbilityGroup allAbilityGroup in group.GetAllAbilityGroups())
		{
			base.Owner.GetOrCreate<UnitPartForbiddenAbilities>().AddEntry(allAbilityGroup, lowerCostException, reason, OnlyCheapestAbilities, weapon);
		}
	}

	protected override void OnDeactivate()
	{
		if (ForbidOtherAbilities)
		{
			base.Owner.GetOrCreate<UnitPartForbiddenAbilities>().RemoveEntry(base.Fact);
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateGroupCooldown evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateGroupCooldown evt)
	{
		if (!CheckAbility(evt.BaseCooldownRule?.Ability, evt.AbilityGroup))
		{
			return;
		}
		ItemEntityWeapon itemEntityWeapon = evt.ConcreteInitiator.GetBodyOptional()?.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon;
		if (!OnlyChosenWeapon || (itemEntityWeapon != null && itemEntityWeapon == base.Owner.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon))
		{
			switch (m_OverrideStrategy)
			{
			case OverrideStrategyType.Override:
				evt.Result = (m_Infinite ? int.MaxValue : m_Value);
				break;
			case OverrideStrategyType.Increase:
				evt.Result = Math.Max(evt.Result + m_Value, 0);
				break;
			case OverrideStrategyType.Reduce:
				evt.Result = Math.Max(evt.Result - m_Value, 0);
				break;
			}
		}
	}

	public void HandleAbilityCooldownStarted(AbilityData ability)
	{
		if (!CheckAbility(ability))
		{
			return;
		}
		if (LimitedCharges)
		{
			ComponentData componentData = RequestSavableData<ComponentData>();
			componentData.Charges--;
			if (componentData.Charges <= 0)
			{
				base.Owner.GetOptional<UnitPartForbiddenAbilities>()?.RemoveEntry(base.Fact);
				if (ActionsAfterCharges.HasActions)
				{
					using (base.Context.GetDataScope(base.OwnerTargetWrapper))
					{
						ActionsAfterCharges.Run();
					}
				}
			}
		}
		if (!ActionsOnOverride.HasActions)
		{
			return;
		}
		PartUnitCombatState combatStateOptional = base.Owner.GetCombatStateOptional();
		if (RefundActionPointCost && combatStateOptional != null)
		{
			AbilityData ability2 = new AbilityData(ability.Blueprint, base.Owner);
			int result = Rulebook.Trigger(new RuleCalculateAbilityActionPointCost(base.Owner, ability2)).Result;
			combatStateOptional.GainYellowPoint(result, base.Context);
		}
		using (base.Context.GetDataScope(base.OwnerTargetWrapper))
		{
			ActionsOnOverride.Run();
		}
	}

	public void HandleGroupCooldownRemoved(BlueprintAbilityGroup group)
	{
	}

	private bool CheckAbility(AbilityData ability, BlueprintAbilityGroup specifiedAbilityGroup = null)
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		List<BlueprintAbilityGroup> affectedGroups = AffectedGroup.GetAllAbilityGroups();
		List<BlueprintAbilityGroup> source = ((specifiedAbilityGroup != null) ? new List<BlueprintAbilityGroup> { specifiedAbilityGroup } : ability.AbilityGroups);
		if (ability != null && source.Any((BlueprintAbilityGroup g) => affectedGroups.Contains(g)) && (!LimitedCharges || componentData.Charges > 0) && (!ForbidOtherAbilities || !ability.IsRestricted) && (!CostRestricted || ability.CalculateActionPointCost() <= MaxActionPointsCost))
		{
			if (FilterGroup != null)
			{
				return ability.AbilityGroups.Contains(FilterGroup);
			}
			return true;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
