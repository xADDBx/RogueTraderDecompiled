using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

[Serializable]
public class HullSlots : IHashable
{
	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemPlasmaDrives> PlasmaDrives;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemVoidShieldGenerator> VoidShieldGenerator;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemWarpDrives> WarpDrives;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemGellerFieldDevice> GellerFieldDevice;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemLifeSustainer> LifeSustainer;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemBridge> Bridge;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemAugerArray> AugerArray;

	[JsonProperty]
	public readonly StarshipEquipmentSlot<BlueprintItemArmorPlating> ArmorPlating;

	[JsonProperty]
	public readonly List<ArsenalSlot> Arsenals = new List<ArsenalSlot>();

	[JsonProperty]
	public readonly List<Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot> WeaponSlots = new List<Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot>();

	public readonly List<ItemSlot> EquipmentSlots = new List<ItemSlot>();

	public BaseUnitEntity Starship { get; private set; }

	public HullSlots(BaseUnitEntity starship)
	{
		Starship = starship;
		PlasmaDrives = new StarshipEquipmentSlot<BlueprintItemPlasmaDrives>(Starship);
		VoidShieldGenerator = new StarshipEquipmentSlot<BlueprintItemVoidShieldGenerator>(Starship);
		WarpDrives = new StarshipEquipmentSlot<BlueprintItemWarpDrives>(Starship);
		GellerFieldDevice = new StarshipEquipmentSlot<BlueprintItemGellerFieldDevice>(Starship);
		LifeSustainer = new StarshipEquipmentSlot<BlueprintItemLifeSustainer>(Starship);
		Bridge = new StarshipEquipmentSlot<BlueprintItemBridge>(Starship);
		AugerArray = new StarshipEquipmentSlot<BlueprintItemAugerArray>(Starship);
		ArmorPlating = new StarshipEquipmentSlot<BlueprintItemArmorPlating>(Starship);
	}

	[JsonConstructor]
	protected HullSlots()
	{
	}

	public void Initialize()
	{
		Warhammer.SpaceCombat.Blueprints.Slots.HullSlots hullSlots = (Starship.Blueprint as BlueprintStarship).HullSlots;
		TryInsertItem(hullSlots.PlasmaDrives, PlasmaDrives);
		TryInsertItem(hullSlots.VoidShieldGenerator, VoidShieldGenerator);
		TryInsertItem(hullSlots.WarpDrives, WarpDrives);
		TryInsertItem(hullSlots.GellerFieldDevice, GellerFieldDevice);
		TryInsertItem(hullSlots.LifeSustainer, LifeSustainer);
		TryInsertItem(hullSlots.Bridge, Bridge);
		TryInsertItem(hullSlots.AugerArray, AugerArray);
		TryInsertItem(hullSlots.ArmorPlating, ArmorPlating);
		CreateArsenals(hullSlots.Arsenals);
		List<WeaponSlotData> weapons = hullSlots.Weapons;
		if (weapons != null)
		{
			foreach (WeaponSlotData item in weapons)
			{
				AddWeapon(item);
			}
		}
		CollectAllSlots();
	}

	private void CollectAllSlots()
	{
		EquipmentSlots.Add(PlasmaDrives);
		EquipmentSlots.Add(VoidShieldGenerator);
		EquipmentSlots.Add(WarpDrives);
		EquipmentSlots.Add(GellerFieldDevice);
		EquipmentSlots.Add(LifeSustainer);
		EquipmentSlots.Add(Bridge);
		EquipmentSlots.Add(AugerArray);
		EquipmentSlots.Add(ArmorPlating);
		EquipmentSlots.AddRange(Arsenals);
		EquipmentSlots.AddRange(WeaponSlots);
	}

	public void TryInsertItem(BlueprintItem bpItem, ItemSlot slot)
	{
		if (!bpItem || (bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		ItemEntity itemEntity = bpItem.CreateEntity();
		if (!slot.CanInsertItem(itemEntity))
		{
			PFLog.Default.Error("'{0}' can't insert item '{1}' to slot '{2}'", Starship.OriginalBlueprint, bpItem, slot.GetType().Name);
			Starship.Inventory.Add(itemEntity);
			return;
		}
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			slot.InsertItem(itemEntity);
		}
	}

	public void CreateArsenals(ReferenceArrayProxy<BlueprintItemArsenal> arsenals)
	{
		foreach (BlueprintItemArsenal item in arsenals)
		{
			ArsenalSlot arsenalSlot = new ArsenalSlot(Starship);
			Arsenals.Add(arsenalSlot);
			TryInsertItem(item, arsenalSlot);
		}
		InitializeArsenals();
	}

	private void InitializeArsenals()
	{
		foreach (ArsenalSlot arsenal in Arsenals)
		{
			arsenal.Initialize(RefreshWeaponSlots);
		}
	}

	private void AddWeapon(WeaponSlotData slotData)
	{
		Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot = new Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot(Starship, slotData);
		ItemEntityStarshipWeapon itemEntityStarshipWeapon = Entity.Initialize(new ItemEntityStarshipWeapon(slotData.Weapon));
		Starship.Inventory.Add(itemEntityStarshipWeapon);
		weaponSlot.InsertItem(itemEntityStarshipWeapon);
		WeaponSlots.Add(weaponSlot);
	}

	public void Subscribe()
	{
		foreach (ItemEntity item in GetAllItemsInternal())
		{
			item.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (ItemEntity item in GetAllItemsInternal())
		{
			item.Unsubscribe();
		}
	}

	public void PreSave()
	{
		EquipmentSlots.Where((ItemSlot s) => s.HasItem).ForEach(delegate(ItemSlot s)
		{
			s.Item.PreSave();
		});
	}

	public void PrePostLoad(BaseUnitEntity starship)
	{
		Starship = starship;
		CollectAllSlots();
		EquipmentSlots.ForEach(delegate(ItemSlot s)
		{
			s.PrePostLoad(Starship);
		});
		TryFixArsenalSlots();
		InitializeArsenals();
	}

	public void PostLoad()
	{
		EquipmentSlots.ForEach(delegate(ItemSlot s)
		{
			s.PostLoad();
		});
	}

	public void Dispose()
	{
		EquipmentSlots.Where((ItemSlot s) => s.HasItem).ForEach(delegate(ItemSlot s)
		{
			s.Item.Dispose();
		});
		WeaponSlots.ForEach(delegate(Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot s)
		{
			s.DisposeArsenals();
		});
	}

	private IEnumerable<ItemEntity> GetAllItemsInternal()
	{
		foreach (ItemSlot equipmentSlot in EquipmentSlots)
		{
			if (equipmentSlot.MaybeItem != null)
			{
				yield return equipmentSlot.MaybeItem;
			}
		}
	}

	private void TryFixArsenalSlots()
	{
		if (Arsenals.Count != 2)
		{
			CreateArsenals(((BlueprintStarship)Starship.Blueprint).HullSlots.Arsenals);
			EquipmentSlots.AddRange(Arsenals);
		}
	}

	private void RefreshWeaponSlots()
	{
		foreach (Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot in WeaponSlots)
		{
			weaponSlot.RefreshArsenals();
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<StarshipEquipmentSlot<BlueprintItemPlasmaDrives>>.GetHash128(PlasmaDrives);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<StarshipEquipmentSlot<BlueprintItemVoidShieldGenerator>>.GetHash128(VoidShieldGenerator);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<StarshipEquipmentSlot<BlueprintItemWarpDrives>>.GetHash128(WarpDrives);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<StarshipEquipmentSlot<BlueprintItemGellerFieldDevice>>.GetHash128(GellerFieldDevice);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<StarshipEquipmentSlot<BlueprintItemLifeSustainer>>.GetHash128(LifeSustainer);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<StarshipEquipmentSlot<BlueprintItemBridge>>.GetHash128(Bridge);
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<StarshipEquipmentSlot<BlueprintItemAugerArray>>.GetHash128(AugerArray);
		result.Append(ref val7);
		Hash128 val8 = ClassHasher<StarshipEquipmentSlot<BlueprintItemArmorPlating>>.GetHash128(ArmorPlating);
		result.Append(ref val8);
		List<ArsenalSlot> arsenals = Arsenals;
		if (arsenals != null)
		{
			for (int i = 0; i < arsenals.Count; i++)
			{
				Hash128 val9 = ClassHasher<ArsenalSlot>.GetHash128(arsenals[i]);
				result.Append(ref val9);
			}
		}
		List<Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot> weaponSlots = WeaponSlots;
		if (weaponSlots != null)
		{
			for (int j = 0; j < weaponSlots.Count; j++)
			{
				Hash128 val10 = ClassHasher<Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot>.GetHash128(weaponSlots[j]);
				result.Append(ref val10);
			}
		}
		return result;
	}
}
