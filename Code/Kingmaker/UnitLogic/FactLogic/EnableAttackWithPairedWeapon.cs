using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("80356eece01ff7949a0c0191afdf7a3a")]
public class EnableAttackWithPairedWeapon : UnitFactComponentDelegate, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IUnitWeaponReimplementedHandler, IEntitySubscriber, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		SwitchAttackWithPairedWeapon(value: true);
		UpdateAbilityWeaponGroups();
	}

	protected override void OnDeactivate()
	{
		SwitchAttackWithPairedWeapon(value: false);
		ResetAbilityWeaponGroups();
	}

	private void SwitchAttackWithPairedWeapon(bool value)
	{
		PartTwoWeaponFighting twoWeaponFightingOptional = base.Owner.GetTwoWeaponFightingOptional();
		if (twoWeaponFightingOptional != null)
		{
			twoWeaponFightingOptional.EnableAttackWithPairedWeapon = value;
		}
	}

	private void UpdateAbilityWeaponGroups()
	{
		BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
		HandsEquipmentSet handsEquipmentSet = base.Owner.GetBodyOptional()?.CurrentHandsEquipmentSet;
		foreach (Ability rawFact in base.Owner.Abilities.RawFacts)
		{
			ItemEntityWeapon itemEntityWeapon = rawFact.Data.SourceWeapon;
			if (itemEntityWeapon == null)
			{
				itemEntityWeapon = (rawFact.Data.SourceItem as ItemEntityShield)?.WeaponComponent;
			}
			if (itemEntityWeapon != null && !itemEntityWeapon.HoldInTwoHands)
			{
				if (itemEntityWeapon.HoldingSlot == handsEquipmentSet?.PrimaryHand)
				{
					rawFact.Data.AbilityGroups.Remove(combatRoot.SecondaryHandAbilityGroup);
				}
				if (itemEntityWeapon.HoldingSlot == handsEquipmentSet?.SecondaryHand)
				{
					rawFact.Data.AbilityGroups.Remove(combatRoot.PrimaryHandAbilityGroup);
				}
				if (itemEntityWeapon.HoldingSlot != handsEquipmentSet?.PrimaryHand && itemEntityWeapon.HoldingSlot != handsEquipmentSet?.SecondaryHand)
				{
					rawFact.Data.AbilityGroups.Remove(combatRoot.PrimaryHandAbilityGroup);
					rawFact.Data.AbilityGroups.Remove(combatRoot.SecondaryHandAbilityGroup);
					rawFact.Data.AbilityGroups.Add(combatRoot.AdditionalLimbsAbilityGroup);
				}
			}
			if (rawFact.Blueprint.IsWeaponAbility && rawFact.Blueprint.Type == AbilityType.Spell && !rawFact.Blueprint.IsFreeAction)
			{
				rawFact.Data.AbilityGroups.Remove(combatRoot.SecondaryHandAbilityGroup);
			}
		}
	}

	private void ResetAbilityWeaponGroups()
	{
		BlueprintCombatRoot combatRoot = BlueprintWarhammerRoot.Instance.CombatRoot;
		HandsEquipmentSet handsEquipmentSet = base.Owner.GetBodyOptional()?.CurrentHandsEquipmentSet;
		foreach (Ability rawFact in base.Owner.Abilities.RawFacts)
		{
			ItemEntityWeapon sourceWeapon = rawFact.Data.SourceWeapon;
			if (sourceWeapon != null && !sourceWeapon.HoldInTwoHands)
			{
				if (sourceWeapon.HoldingSlot == handsEquipmentSet?.PrimaryHand)
				{
					rawFact.Data.AbilityGroups.Add(combatRoot.SecondaryHandAbilityGroup);
				}
				if (sourceWeapon.HoldingSlot == handsEquipmentSet?.SecondaryHand)
				{
					rawFact.Data.AbilityGroups.Add(combatRoot.PrimaryHandAbilityGroup);
				}
				if (sourceWeapon.HoldingSlot != handsEquipmentSet?.PrimaryHand && sourceWeapon.HoldingSlot != handsEquipmentSet?.SecondaryHand)
				{
					rawFact.Data.AbilityGroups.Add(combatRoot.PrimaryHandAbilityGroup);
					rawFact.Data.AbilityGroups.Add(combatRoot.SecondaryHandAbilityGroup);
					rawFact.Data.AbilityGroups.Remove(combatRoot.AdditionalLimbsAbilityGroup);
				}
			}
			if (rawFact.Blueprint.IsWeaponAbility && rawFact.Blueprint.Type == AbilityType.Spell && !rawFact.Blueprint.IsFreeAction)
			{
				rawFact.Data.AbilityGroups.Add(combatRoot.SecondaryHandAbilityGroup);
			}
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		UpdateAbilityWeaponGroups();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateAbilityWeaponGroups();
	}

	public void HandleUnitWeaponReimplemented()
	{
		UpdateAbilityWeaponGroups();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
