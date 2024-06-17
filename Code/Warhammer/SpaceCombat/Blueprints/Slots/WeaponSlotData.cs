using System;
using Kingmaker.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints.Slots;

[Serializable]
public class WeaponSlotData
{
	public WeaponSlotType Type;

	public BlueprintStarshipWeaponReference Weapon;

	[Tooltip("Weapon battery offset in cell units. Used for Port, Starboard and Dorsal slots.\n0 means firing arc starts from cell closest to prow, 1 - next to it and so on.")]
	[ShowIf("IsBoardsideSlot")]
	public int OffsetFromProw;

	[Tooltip("Weapon battery width in cell units. Used for Port, Starboard and Dorsal slots.\n0 means default width covering all the starship length. Exceeding value will be automatically clamped to the starship length.")]
	[ShowIf("IsBoardsideSlot")]
	public int Width;

	private bool IsBoardsideSlot
	{
		get
		{
			if (Type != WeaponSlotType.Prow)
			{
				return Type != WeaponSlotType.Keel;
			}
			return false;
		}
	}
}
