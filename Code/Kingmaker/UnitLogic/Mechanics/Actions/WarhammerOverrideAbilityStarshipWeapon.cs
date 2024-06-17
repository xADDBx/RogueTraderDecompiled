using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("08caf9293baf6e54cafcfca35b4a5259")]
public class WarhammerOverrideAbilityStarshipWeapon : MechanicEntityFactComponentDelegate, IHashable
{
	public enum OverrideMode
	{
		WithItem,
		FromWeaponWithType,
		FromPlasmaDrives
	}

	[SerializeField]
	private OverrideMode overrideMode;

	[SerializeField]
	[ShowIf("NeedShowWeaponType")]
	private StarshipWeaponType m_WeaponType;

	[SerializeField]
	[ShowIf("NeedShowItem")]
	private BlueprintStarshipWeaponReference m_StarshipWeapon;

	[SerializeField]
	private bool m_UseAlternativeAmmo;

	public bool NeedShowItem => overrideMode == OverrideMode.WithItem;

	public bool NeedShowWeaponType => overrideMode == OverrideMode.FromWeaponWithType;

	public BlueprintStarshipWeapon StarshipWeapon(StarshipEntity starshipEntity)
	{
		if (starshipEntity == null)
		{
			return null;
		}
		BlueprintStarship blueprint = starshipEntity.Blueprint;
		return overrideMode switch
		{
			OverrideMode.WithItem => m_StarshipWeapon?.Get(), 
			OverrideMode.FromWeaponWithType => GetWeaponWithType(blueprint, m_WeaponType), 
			OverrideMode.FromPlasmaDrives => Drives(blueprint)?.ExplosionWeapon, 
			_ => null, 
		};
	}

	public BlueprintStarshipAmmo StarshipWeaponAmmo(StarshipEntity starshipEntity)
	{
		if (starshipEntity == null)
		{
			return null;
		}
		if (overrideMode == OverrideMode.FromPlasmaDrives)
		{
			return Drives(starshipEntity.Blueprint)?.ExplosionAmmo;
		}
		if (!m_UseAlternativeAmmo)
		{
			return StarshipWeapon(starshipEntity)?.DefaultAmmo;
		}
		return StarshipWeapon(starshipEntity)?.AlternateAmmo;
	}

	private static BlueprintItemPlasmaDrives Drives(BlueprintStarship starship)
	{
		return starship.HullSlots.PlasmaDrives;
	}

	private static BlueprintStarshipWeapon GetWeaponWithType(BlueprintStarship starship, StarshipWeaponType weaponType)
	{
		return (from s in starship.Weapons
			select s.Weapon into w
			where w != null && w.Get().WeaponType == weaponType
			select w).FirstOrDefault()?.Get();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
