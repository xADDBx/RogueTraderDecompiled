using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityShield : ItemEntity<BlueprintItemShield>, IHashable
{
	private int m_ArmorEnchantmentsRuntimeVersion;

	private int m_WeaponEnchantmentsRuntimeVersion;

	[JsonProperty]
	public ItemEntityArmor ArmorComponent { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public ItemEntityWeapon WeaponComponent { get; private set; }

	public override bool InstantiateEnchantments => false;

	public override int EnchantmentValue => ArmorComponent.EnchantmentValue;

	public ItemEntityShield([NotNull] BlueprintItemShield bpItem)
		: base(bpItem)
	{
		ArmorComponent = Entity.Initialize(new ItemEntityArmor(bpItem.ArmorComponent, this));
		if (bpItem.WeaponComponent != null)
		{
			WeaponComponent = Entity.Initialize(new ItemEntityWeapon(bpItem.WeaponComponent, this));
		}
	}

	protected ItemEntityShield(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void RemoveEnchantment(ItemEnchantment enchantment)
	{
		base.RemoveEnchantment(enchantment);
		ArmorComponent.RemoveEnchantment(enchantment);
		WeaponComponent?.RemoveEnchantment(enchantment);
	}

	protected override void UpdateCachedEnchantments(List<ItemEnchantment> enchantmentsList)
	{
		bool num = ArmorComponent.EnchantmentsCollection != null && ArmorComponent.EnchantmentsCollection.RuntimeVersion != m_ArmorEnchantmentsRuntimeVersion;
		bool flag = WeaponComponent?.EnchantmentsCollection != null && WeaponComponent?.EnchantmentsCollection.RuntimeVersion != m_WeaponEnchantmentsRuntimeVersion;
		if (num || flag)
		{
			enchantmentsList.Clear();
			if (ArmorComponent.EnchantmentsCollection != null)
			{
				enchantmentsList.AddRange(ArmorComponent.Enchantments);
				m_ArmorEnchantmentsRuntimeVersion = ArmorComponent.EnchantmentsCollection.RuntimeVersion;
			}
			if (WeaponComponent?.EnchantmentsCollection != null)
			{
				enchantmentsList.AddRange(WeaponComponent.Enchantments);
				m_WeaponEnchantmentsRuntimeVersion = WeaponComponent.EnchantmentsCollection.RuntimeVersion;
			}
		}
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		ArmorComponent.OnDidEquipped(wielder);
		ArmorComponent.HoldingSlot = base.HoldingSlot;
		WeaponComponent?.OnDidEquipped(wielder);
		if (WeaponComponent != null)
		{
			WeaponComponent.HoldingSlot = base.HoldingSlot;
		}
	}

	public override void OnWillUnequip()
	{
		ArmorComponent.OnWillUnequip();
		ArmorComponent.HoldingSlot = null;
		WeaponComponent?.OnWillUnequip();
		if (WeaponComponent != null)
		{
			WeaponComponent.HoldingSlot = null;
		}
		base.OnWillUnequip();
	}

	protected override bool CanBeEquippedInternal(MechanicEntity owner)
	{
		if (base.CanBeEquippedInternal(owner))
		{
			return owner.GetProficienciesOptional()?.Contains(base.Blueprint.ArmorComponent.ProficiencyGroup) ?? true;
		}
		return false;
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		ArmorComponent.Subscribe();
		WeaponComponent?.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		base.OnUnsubscribe();
		ArmorComponent.Unsubscribe();
		WeaponComponent?.Unsubscribe();
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		ArmorComponent.PreSave();
		WeaponComponent?.PreSave();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		base.EnchantmentsCollection?.Dispose();
		ArmorComponent.PostLoad(this);
		WeaponComponent?.PostLoad(this);
		bool flag = base.Wielder != null;
		if (ArmorComponent.Blueprint != base.Blueprint.ArmorComponent)
		{
			ItemEntityArmor armorComponent = ArmorComponent;
			ArmorComponent = new ItemEntityArmor(base.Blueprint.ArmorComponent, this);
			if (base.IsIdentified)
			{
				ArmorComponent.Identify();
			}
			if (flag)
			{
				armorComponent.OnWillUnequip();
				ArmorComponent.OnDidEquipped(base.Wielder);
			}
			PFLog.Default.Warning($"Replaced ArmorComponent in shield {base.Blueprint}: {armorComponent.Blueprint} --> {ArmorComponent.Blueprint}");
			armorComponent.Dispose();
		}
		if (WeaponComponent?.Blueprint != base.Blueprint.WeaponComponent)
		{
			ItemEntityWeapon weaponComponent = WeaponComponent;
			WeaponComponent = (base.Blueprint.WeaponComponent ? Entity.Initialize(new ItemEntityWeapon(base.Blueprint.WeaponComponent, this)) : null);
			if (base.IsIdentified)
			{
				WeaponComponent?.Identify();
			}
			if (flag)
			{
				weaponComponent?.OnWillUnequip();
				WeaponComponent?.OnDidEquipped(base.Wielder);
			}
			PFLog.Default.Warning(string.Format("Replaced WeaponComponent in shield {0}: {1} --> {2}", base.Blueprint, weaponComponent?.Blueprint.ToString() ?? "<null>", WeaponComponent?.Blueprint.ToString() ?? "<null>"));
			weaponComponent?.Dispose();
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		ArmorComponent.Dispose();
		WeaponComponent?.Dispose();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemEntityArmor>.GetHash128(ArmorComponent);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ItemEntityWeapon>.GetHash128(WeaponComponent);
		result.Append(ref val3);
		return result;
	}
}
