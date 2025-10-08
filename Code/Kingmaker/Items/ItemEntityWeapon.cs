using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechadendrites;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityWeapon : ItemEntity<BlueprintItemWeapon>, IHashable
{
	[JsonProperty]
	public readonly ItemEntityWeapon Second;

	private bool m_WasEquippedBefore;

	public bool IsMonkUnarmedStrike;

	[JsonIgnore]
	public RandomShuffleSequence ProjectileLocatorIndexSequence;

	[JsonProperty]
	public bool ForceSecondary { get; set; }

	[JsonProperty]
	public bool IsSecondPartOfDoubleWeapon { get; private set; }

	[JsonProperty]
	public int CurrentUsedBarrel { get; set; }

	public ItemEntityShield Shield { get; private set; }

	public bool IsShield => Shield != null;

	public override bool IsLootable
	{
		get
		{
			if (base.IsLootable)
			{
				return !base.Blueprint.IsNatural;
			}
			return false;
		}
	}

	public int AttackRange => this.GetWeaponStats().ResultMaxDistance;

	public int AttackOptimalRange => this.GetWeaponStats().ResultOptimalDistance;

	public override bool IsPartOfAnotherItem
	{
		get
		{
			if (Shield == null)
			{
				return IsSecondPartOfDoubleWeapon;
			}
			return true;
		}
	}

	[JsonProperty]
	public int CurrentAmmo { get; set; }

	public bool HoldInTwoHands
	{
		get
		{
			if (base.Owner is UnitEntity unitEntity)
			{
				if (unitEntity.GetOptional<UnitPartMechadendrites>() != null)
				{
					return false;
				}
				if ((bool)unitEntity.Features.CarryShotgunInOneHand && base.Blueprint.Classification == WeaponClassification.Shotgun)
				{
					return false;
				}
			}
			return base.Blueprint.IsTwoHanded;
		}
	}

	public WeaponAnimationStyle GetAnimationStyle(bool forDollRoom = false, MechanicEntity owner = null)
	{
		if (IsShield)
		{
			return WeaponAnimationStyle.Shield;
		}
		if (owner == null)
		{
			owner = base.Owner;
		}
		if (owner != null && base.Blueprint.Classification == WeaponClassification.Shotgun && (bool)owner.Features.CarryShotgunInOneHand)
		{
			return WeaponAnimationStyle.ShotgunOneHanded;
		}
		return base.Blueprint.VisualParameters.AnimStyle;
	}

	public ItemEntityWeapon([NotNull] BlueprintItemWeapon bpItem, ItemEntityShield shield = null)
		: base(bpItem)
	{
		Shield = shield;
		CurrentAmmo = base.Blueprint.WarhammerMaxAmmo;
	}

	protected ItemEntityWeapon(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnReapplyFactsForWielder()
	{
		base.OnReapplyFactsForWielder();
		ReapplyAbilitiesImpl();
	}

	public void ReapplyAbilities()
	{
		ReapplyAbilitiesImpl();
	}

	private void ReapplyAbilitiesImpl()
	{
		base.Abilities.ForEach(delegate(Ability v)
		{
			base.Wielder.Facts.Remove(v);
		});
		base.Abilities.Clear();
		MechanicEntity wielder = base.Wielder;
		if (base.Blueprint != null && wielder != null)
		{
			base.Abilities.AddRange((from i in (from i in base.Blueprint.WeaponAbilities.AllWithIndex
					where i.Slot.Ability != null
					select ((WeaponAbility Slot, int SlotIndex, EntityFact Source))(Slot: i.Slot, SlotIndex: i.Index, Source: null)).Concat(GetExtraAbilitiesFromWielder())
				orderby i.Slot.Type == WeaponAbilityType.Reload
				select AddAbility(i.Slot, i.SlotIndex, i.Source)).NotNull());
			EventBus.RaiseEvent((IMechanicEntity)wielder, (Action<IUnitWeaponReimplementedHandler>)delegate(IUnitWeaponReimplementedHandler h)
			{
				h.HandleUnitWeaponReimplemented();
			}, isCheckRuntime: true);
		}
	}

	private IEnumerable<(WeaponAbility Slot, int SlotIndex, EntityFact Source)> GetExtraAbilitiesFromWielder()
	{
		foreach (EntityFact fact in base.Wielder.Facts.GetAll<EntityFact>())
		{
			AddAbilitiesToCurrentWeapon addAbilities = fact.GetComponent<AddAbilitiesToCurrentWeapon>();
			if (addAbilities?.ShouldAddAbilities(this) ?? false)
			{
				for (int abilityIndex = 0; abilityIndex < addAbilities.WeaponAbilities.Count; abilityIndex++)
				{
					WeaponAbility item = addAbilities.WeaponAbilities[abilityIndex];
					yield return (Slot: item, SlotIndex: abilityIndex, Source: fact);
				}
			}
		}
	}

	private Ability AddAbility(WeaponAbility weaponAbility, int itemSlotIndex, EntityFact itemSlotSource)
	{
		Ability ability = base.Wielder.Facts.Add(new Ability(weaponAbility.Ability, base.Wielder));
		if (ability == null)
		{
			return null;
		}
		ability.Data.ItemSlotSource = itemSlotSource;
		ability.Data.ItemSlotIndex = itemSlotIndex;
		ItemEntity itemEntity = (ItemEntity)(((object)Shield) ?? ((object)this));
		ability.AddSource(itemEntity);
		itemEntity.Abilities.RemoveAll((Ability o) => o.Blueprint == ability.Blueprint);
		itemEntity.Abilities.Add(ability);
		return ability;
	}

	private void EnsureAbilitiesCoherency()
	{
		BlueprintItemWeapon blueprint = base.Blueprint;
		if (blueprint == null)
		{
			return;
		}
		bool flag = false;
		foreach (var item2 in blueprint.WeaponAbilities.AllWithIndex)
		{
			int index = item2.Index;
			WeaponAbility item = item2.Slot;
			Ability ability = base.Abilities.FirstItem((Ability i) => i != null && i.Data.ItemSlotIndex == index && i.Data.ItemSlotSource == null);
			flag = (ability == null && !item.IsNone && item.Ability != null) || (ability != null && (item.IsNone || item.Ability == null));
			if (flag)
			{
				break;
			}
			if (ability != null)
			{
				ability.Data.ItemSlotIndex = index;
			}
		}
		if (flag)
		{
			ReapplyAbilitiesImpl();
		}
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		Second?.OnDidEquipped(wielder);
		if (!wielder.IsInCombat)
		{
			Reload();
		}
	}

	public override void OnWillUnequip()
	{
		Second?.OnWillUnequip();
		base.OnWillUnequip();
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		Second?.PreSave();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		Second?.PostLoad();
	}

	protected override void OnDidPostLoad()
	{
		base.OnDidPostLoad();
		EnsureAbilitiesCoherency();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Second?.Dispose();
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		Second?.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		Second?.Unsubscribe();
		base.OnUnsubscribe();
	}

	public void PostLoad(ItemEntityShield shield)
	{
		Shield = shield;
		PostLoad();
	}

	public void Reload()
	{
		CurrentAmmo = base.Blueprint.WarhammerMaxAmmo;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = ForceSecondary;
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<ItemEntityWeapon>.GetHash128(Second);
		result.Append(ref val3);
		bool val4 = IsSecondPartOfDoubleWeapon;
		result.Append(ref val4);
		int val5 = CurrentUsedBarrel;
		result.Append(ref val5);
		int val6 = CurrentAmmo;
		result.Append(ref val6);
		return result;
	}
}
