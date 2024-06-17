using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class EncumbranceHelper
{
	public struct CarryingCapacity
	{
		public int Light { get; internal set; }

		public int Medium { get; internal set; }

		public int Heavy { get; internal set; }

		public float CurrentWeight { get; internal set; }

		public string GetCurrentWeightText()
		{
			return $"{Mathf.Max(CurrentWeight, 0f):0.#}";
		}

		public string GetEncumbranceText()
		{
			return UIStrings.Instance.Tooltips.EncumbranceStatus[(int)GetEncumbrance()];
		}

		public Encumbrance GetEncumbrance(float weight)
		{
			if (BuildModeUtility.IsDevelopment && CheatsCommon.IgnoreEncumbrance)
			{
				return Encumbrance.Light;
			}
			if (weight <= (float)Light)
			{
				return Encumbrance.Light;
			}
			if (weight <= (float)Medium)
			{
				return Encumbrance.Medium;
			}
			if (weight <= (float)Heavy)
			{
				return Encumbrance.Heavy;
			}
			return Encumbrance.Overload;
		}

		public Encumbrance GetEncumbrance()
		{
			return GetEncumbrance(CurrentWeight);
		}

		public bool Equals(CarryingCapacity other)
		{
			if (Light == other.Light && Medium == other.Medium && Heavy == other.Heavy)
			{
				return CurrentWeight.Equals(other.CurrentWeight);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is CarryingCapacity)
			{
				return Equals((CarryingCapacity)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((Light * 397) ^ Medium) * 397) ^ Heavy) * 397) ^ CurrentWeight.GetHashCode();
		}

		public static CarryingCapacity operator +(CarryingCapacity c1, CarryingCapacity c2)
		{
			CarryingCapacity result = default(CarryingCapacity);
			result.CurrentWeight = c1.CurrentWeight + c2.CurrentWeight;
			result.Light = c1.Light + c2.Light;
			result.Medium = c1.Medium + c2.Medium;
			result.Heavy = c1.Heavy + c2.Heavy;
			return result;
		}

		public static bool operator ==(CarryingCapacity c1, CarryingCapacity c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(CarryingCapacity c1, CarryingCapacity c2)
		{
			return !c1.Equals(c2);
		}
	}

	public static int GetHeavy(int str)
	{
		if (str < 10)
		{
			return str * 10;
		}
		double num = Math.Pow(Math.Pow(4.0, 0.1), str - 10);
		double num2 = 5.0 * Math.Pow(2.0, Math.Truncate(Math.Log(num, 2.0)));
		return (int)Math.Round(Math.Truncate(100.0 * num / num2 + 0.5) * num2);
	}

	public static int GetMedium(int str)
	{
		return GetMedium(str, GetHeavy(str));
	}

	public static int GetLight(int str)
	{
		return GetLight(str, GetHeavy(str));
	}

	private static int GetMedium(int str, int heavy)
	{
		return (int)Math.Ceiling((double)heavy * 2.0 / 3.0);
	}

	private static int GetLight(int str, int heavy)
	{
		return (int)Math.Ceiling((double)heavy / 3.0);
	}

	public static CarryingCapacity GetCarryingCapacity(int str)
	{
		CarryingCapacity result = default(CarryingCapacity);
		result.Light = GetLight(str);
		result.Medium = GetMedium(str);
		result.Heavy = GetHeavy(str);
		return result;
	}

	public static CarryingCapacity GetCarryingCapacity(IBaseUnitEntity unit)
	{
		return GetCarryingCapacity((BaseUnitEntity)unit);
	}

	public static CarryingCapacity GetCarryingCapacity(BaseUnitEntity unit)
	{
		int num = unit.GetOptional<UnitPartAdditionalEncumbrance>()?.AdditionalEncumbrance ?? 0;
		int heavy = GetHeavy(unit.Attributes.WarhammerStrength);
		CarryingCapacity result = default(CarryingCapacity);
		result.CurrentWeight = unit.Body.EquipmentWeight;
		result.Light = GetLight(unit.Attributes.WarhammerStrength, heavy) + num;
		result.Medium = GetMedium(unit.Attributes.WarhammerStrength, heavy) + num;
		result.Heavy = heavy + num;
		return result;
	}

	public static Encumbrance GetEncumbrance(IBaseUnitEntity unit)
	{
		return GetEncumbrance((BaseUnitEntity)unit);
	}

	public static Encumbrance GetEncumbrance(BaseUnitEntity unit)
	{
		return GetCarryingCapacity(unit).GetEncumbrance();
	}

	public static CarryingCapacity GetPartyCarryingCapacity()
	{
		using (ProfileScope.New("Encumbrance.GetPartyCarryingCapacity"))
		{
			CarryingCapacity result = default(CarryingCapacity);
			Player player = Game.Instance.Player;
			using (ProfileScope.New("PartyCapacity"))
			{
				foreach (BaseUnitEntity partyAndPet in player.PartyAndPets)
				{
					result += GetCarryingCapacity(partyAndPet);
				}
			}
			using (ProfileScope.New("DetachedCapacity"))
			{
				foreach (BaseUnitEntity item in player.PartyAndPetsDetached)
				{
					result += GetCarryingCapacity(item);
				}
			}
			result.CurrentWeight = player.Inventory.Weight - GetAllCharactersEquipmentWeight();
			return result;
		}
	}

	public static CarryingCapacity GetPartyCarryingCapacity([NotNull] IEnumerable<UnitReference> partyOverride)
	{
		CarryingCapacity result = default(CarryingCapacity);
		foreach (UnitReference item in partyOverride)
		{
			if (item.Entity != null)
			{
				result += GetCarryingCapacity(item.Entity.ToIBaseUnitEntity());
			}
		}
		result.CurrentWeight = Game.Instance.Player.Inventory.Weight - GetAllCharactersEquipmentWeight();
		return result;
	}

	private static float GetAllCharactersEquipmentWeight()
	{
		float num = 0f;
		using (ProfileScope.New("EquipmentWeight"))
		{
			foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
			{
				if (allCharacter.Inventory.IsPlayerInventory)
				{
					num += allCharacter.Body.EquipmentWeight;
				}
			}
			return num;
		}
	}

	public static Encumbrance GetPartyEncumbrance()
	{
		return GetPartyCarryingCapacity().GetEncumbrance();
	}

	public static Encumbrance GetPartyEncumbrance([NotNull] IEnumerable<UnitReference> partyOverride)
	{
		return GetPartyCarryingCapacity(partyOverride).GetEncumbrance();
	}

	public static Feet GetPartyNonCombatSpeed(Encumbrance encumbrance)
	{
		switch (encumbrance)
		{
		case Encumbrance.Light:
		case Encumbrance.Medium:
			return 30.Feet();
		case Encumbrance.Heavy:
			return 20.Feet();
		case Encumbrance.Overload:
			return 10.Feet();
		default:
			return 30.Feet();
		}
	}
}
