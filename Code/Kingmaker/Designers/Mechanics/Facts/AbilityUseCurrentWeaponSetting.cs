using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("256c41efd74f4792a30353c4cf1cc1b2")]
public class AbilityUseCurrentWeaponSetting : MechanicEntityFactComponentDelegate, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>, IHashable
{
	public bool useSecondWeapon;

	private MechanicEntity ConcreteOwner => base.Owner;

	protected override void OnActivate()
	{
		ResetSettings();
	}

	void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot == (useSecondWeapon ? ConcreteOwner.GetSecondaryHandWeapon()?.HoldingSlot : ConcreteOwner.GetFirstWeapon()?.HoldingSlot))
		{
			ResetSettings();
		}
	}

	void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet()
	{
		ResetSettings();
	}

	private void ResetSettings()
	{
		if (!(base.Fact is Ability ability))
		{
			return;
		}
		ItemEntityWeapon itemEntityWeapon = ((!useSecondWeapon) ? ConcreteOwner.GetFirstWeapon() : ConcreteOwner.GetSecondaryHandWeapon());
		ability.Data.OverrideWeapon = itemEntityWeapon;
		if (itemEntityWeapon != null)
		{
			BlueprintAbilityFXSettings fXSettingsOverride = itemEntityWeapon.Abilities.FirstItem((Ability x) => x.Blueprint.IsBurst)?.Data.FXSettings;
			ability.Data.FXSettingsOverride = fXSettingsOverride;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
