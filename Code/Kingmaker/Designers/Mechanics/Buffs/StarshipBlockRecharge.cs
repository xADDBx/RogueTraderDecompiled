using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("Blocks recharging of selected starship weapon type")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("92510e08f7ed17544a7bc96d6941fb19")]
public class StarshipBlockRecharge : UnitBuffComponentDelegate, ITurnBasedModeHandler, ISubscriber, IHashable
{
	public StarshipWeaponType WeaponType;

	public bool CheckSlot;

	[ShowIf("CheckSlot")]
	public WeaponSlotType slot;

	public bool Match(ItemEntityStarshipWeapon weapon)
	{
		if (WeaponType == weapon.Blueprint.WeaponType)
		{
			if (CheckSlot)
			{
				return slot == weapon.WeaponSlot.Type;
			}
			return true;
		}
		return false;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased || !(base.Owner is StarshipEntity starshipEntity))
		{
			return;
		}
		foreach (ItemEntityStarshipWeapon item in starshipEntity.Hull.Weapons.Where((ItemEntityStarshipWeapon weapon) => Match(weapon)))
		{
			item.Charges = 0;
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
