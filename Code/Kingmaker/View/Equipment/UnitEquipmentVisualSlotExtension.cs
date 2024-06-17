namespace Kingmaker.View.Equipment;

public static class UnitEquipmentVisualSlotExtension
{
	public static readonly string[] BoneNames = new string[13]
	{
		"None", "R_front_weapon_slot_01_ADJ", "R_front_weapon_slot_02_ADJ", "L_front_weapon_slot_05_ADJ", "L_front_weapon_slot_04_ADJ", "C_front_weapon_slot_03_ADJ", "L_back_weapon_slot_07_ADJ", "L_back_weapon_slot_07_ADJ", "R_back_weapon_slot_06_ADJ", "R_back_weapon_slot_09_ADJ",
		"C_back_w_____slot_08", "C_back_w_____slot_11", "R_MechadendriteWeaponBone"
	};

	public static readonly UnitEquipmentAnimationSlotType[] AnimSlots = new UnitEquipmentAnimationSlotType[13]
	{
		UnitEquipmentAnimationSlotType.None,
		UnitEquipmentAnimationSlotType.FrontRight,
		UnitEquipmentAnimationSlotType.FrontRight,
		UnitEquipmentAnimationSlotType.FrontLeft,
		UnitEquipmentAnimationSlotType.FrontLeft,
		UnitEquipmentAnimationSlotType.None,
		UnitEquipmentAnimationSlotType.BackLeft,
		UnitEquipmentAnimationSlotType.BackLeft,
		UnitEquipmentAnimationSlotType.BackRight,
		UnitEquipmentAnimationSlotType.BackRight,
		UnitEquipmentAnimationSlotType.BackCenter,
		UnitEquipmentAnimationSlotType.None,
		UnitEquipmentAnimationSlotType.Mechadendrite
	};

	public static string GetBoneName(this UnitEquipmentVisualSlotType slot)
	{
		return BoneNames[(int)slot];
	}

	public static UnitEquipmentAnimationSlotType GetAnimSlot(this UnitEquipmentVisualSlotType slot)
	{
		return AnimSlots[(int)slot];
	}

	public static bool IsFront(this UnitEquipmentVisualSlotType slot)
	{
		if (slot != UnitEquipmentVisualSlotType.RightFront01)
		{
			return slot == UnitEquipmentVisualSlotType.LeftFront01;
		}
		return true;
	}

	public static bool IsBack(this UnitEquipmentVisualSlotType slot)
	{
		if (slot != UnitEquipmentVisualSlotType.LeftBack01 && slot != UnitEquipmentVisualSlotType.RightBack01)
		{
			return slot == UnitEquipmentVisualSlotType.Shield;
		}
		return true;
	}

	public static bool IsLeft(this UnitEquipmentVisualSlotType slot)
	{
		if (slot != UnitEquipmentVisualSlotType.LeftBack01)
		{
			return slot == UnitEquipmentVisualSlotType.LeftFront01;
		}
		return true;
	}

	public static bool IsRight(this UnitEquipmentVisualSlotType slot)
	{
		if (slot != UnitEquipmentVisualSlotType.RightBack01)
		{
			return slot == UnitEquipmentVisualSlotType.RightFront01;
		}
		return true;
	}
}
