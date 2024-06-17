using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("1fddf23bccefc9b49b5450c0fe823062")]
public class BlueprintStarshipAmmo : BlueprintItemEquipment
{
	public StarshipWeaponType WeaponType;

	public int Damage;

	public int MaxDamage;

	public bool IsIgnoreEvasion;

	public bool IsAE;

	public DamageTypeDescription DamageType;

	[SerializeField]
	private BlueprintProjectileReference m_ShotProjectile;

	public BlueprintBuffReference[] HitEffects;

	public BlueprintBuffReference[] CriticalHitEffects;

	public override ItemsItemType ItemType => ItemsItemType.StarshipAmmo;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public BlueprintProjectile ShotProjectile => m_ShotProjectile?.Get();
}
