using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fab5c718491ba8343aec71c233f7c8bd")]
public class StarshipAutoreloadWeapon : UnitFactComponentDelegate, IItemChargesHandler, ISubscriber, IHashable
{
	public StarshipWeaponType WeaponType;

	public ActionList ActionsOnReload;

	public void HandleItemChargeSpent(ItemEntity item)
	{
		if (!(item is ItemEntityStarshipWeapon itemEntityStarshipWeapon) || itemEntityStarshipWeapon.Blueprint.WeaponType != WeaponType)
		{
			return;
		}
		itemEntityStarshipWeapon.Reload();
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(ActionsOnReload, base.Owner.ToITargetWrapper());
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
