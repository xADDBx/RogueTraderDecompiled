using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Parts;

public class StarShipUnitPartRapidReload : BaseUnitPart, IHashable
{
	[JsonProperty]
	private readonly Dictionary<ItemEntityStarshipWeapon, bool> activatedWeapons = new Dictionary<ItemEntityStarshipWeapon, bool>();

	public void ClearActivationInfo()
	{
		activatedWeapons.Clear();
	}

	public void AbilityActivationNotification(ItemEntityStarshipWeapon weapon, bool penalted)
	{
		activatedWeapons.Add(weapon, penalted);
	}

	public bool HasWeaponsToReload(bool allowPenalted)
	{
		return activatedWeapons.Any((KeyValuePair<ItemEntityStarshipWeapon, bool> w) => allowPenalted || !w.Value);
	}

	public bool ReloadWeapons(bool allowPenalted)
	{
		activatedWeapons.Where((KeyValuePair<ItemEntityStarshipWeapon, bool> w) => !w.Value || allowPenalted).ForEach(delegate(KeyValuePair<ItemEntityStarshipWeapon, bool> w)
		{
			w.Key.Reload();
		});
		bool result = activatedWeapons.Any((KeyValuePair<ItemEntityStarshipWeapon, bool> w) => w.Value);
		ClearActivationInfo();
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<ItemEntityStarshipWeapon, bool> dictionary = activatedWeapons;
		if (dictionary != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<ItemEntityStarshipWeapon, bool> item in dictionary)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = ClassHasher<ItemEntityStarshipWeapon>.GetHash128(item.Key);
				hash.Append(ref val3);
				bool obj = item.Value;
				Hash128 val4 = UnmanagedHasher<bool>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}
}
