using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Warhammer.SpaceCombat.Blueprints.Progression;

[TypeId("53dab683877e45f19422861f1b0d603b")]
public class BlueprintShipComponentsUnlockTable : BlueprintScriptableObject
{
	[Serializable]
	public struct UnlockTable
	{
		[SerializeField]
		public int Level;

		[SerializeField]
		public BlueprintStarshipItemReference ShipComponent;

		[SerializeField]
		[Tooltip("Fill this if Item is weapon")]
		public WeaponSlotType WeaponSlotType;

		[SerializeField]
		[Tooltip("If insert prow, then set on which side to equip. Uncheck - Left. Check - Right")]
		public bool ProwInsertRightSlot;
	}

	[SerializeField]
	public UnlockTable[] Table;

	public void UpdateShipEquipment(int level)
	{
		foreach (UnlockTable item in Table.Where((UnlockTable x) => x.Level == level))
		{
			EquipItem(item);
		}
	}

	private void EquipItem(UnlockTable data)
	{
		Warhammer.SpaceCombat.StarshipLogic.Equipment.HullSlots hullSlots = Game.Instance.Player.PlayerShip.Hull.HullSlots;
		BlueprintStarshipItem blueprintStarshipItem = data.ShipComponent.Get();
		if (!(blueprintStarshipItem is BlueprintStarshipWeapon weapon))
		{
			if (!(blueprintStarshipItem is BlueprintItemPlasmaDrives bpItem))
			{
				if (!(blueprintStarshipItem is BlueprintItemVoidShieldGenerator bpItem2))
				{
					if (!(blueprintStarshipItem is BlueprintItemWarpDrives bpItem3))
					{
						if (!(blueprintStarshipItem is BlueprintItemGellerFieldDevice bpItem4))
						{
							if (!(blueprintStarshipItem is BlueprintItemLifeSustainer bpItem5))
							{
								if (!(blueprintStarshipItem is BlueprintItemBridge bpItem6))
								{
									if (!(blueprintStarshipItem is BlueprintItemAugerArray bpItem7))
									{
										if (blueprintStarshipItem is BlueprintItemArmorPlating bpItem8)
										{
											hullSlots.TryInsertItem(bpItem8, hullSlots.ArmorPlating);
										}
									}
									else
									{
										hullSlots.TryInsertItem(bpItem7, hullSlots.AugerArray);
									}
								}
								else
								{
									hullSlots.TryInsertItem(bpItem6, hullSlots.Bridge);
								}
							}
							else
							{
								hullSlots.TryInsertItem(bpItem5, hullSlots.LifeSustainer);
							}
						}
						else
						{
							hullSlots.TryInsertItem(bpItem4, hullSlots.GellerFieldDevice);
						}
					}
					else
					{
						hullSlots.TryInsertItem(bpItem3, hullSlots.WarpDrives);
					}
				}
				else
				{
					hullSlots.TryInsertItem(bpItem2, hullSlots.VoidShieldGenerator);
				}
			}
			else
			{
				hullSlots.TryInsertItem(bpItem, hullSlots.PlasmaDrives);
			}
		}
		else
		{
			EquipWeapon(weapon, data.WeaponSlotType, data.ProwInsertRightSlot);
		}
	}

	private void EquipWeapon(BlueprintStarshipWeapon weapon, WeaponSlotType type, bool prowRightSlot)
	{
		Warhammer.SpaceCombat.StarshipLogic.Equipment.HullSlots hullSlots = Game.Instance.Player.PlayerShip.Hull.HullSlots;
		List<WeaponSlot> list = hullSlots.WeaponSlots.Where((WeaponSlot x) => x.Type == type).ToList();
		switch (list.Count)
		{
		case 1:
			hullSlots.TryInsertItem(weapon, list[0]);
			break;
		case 2:
			hullSlots.TryInsertItem(weapon, prowRightSlot ? list[1] : list[0]);
			break;
		}
	}
}
