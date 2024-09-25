using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("ca5f3cfd04654b9489e1b267fc30a044")]
public class BlueprintItemPlasmaDrives : BlueprintStarshipItem
{
	[Header("Propulsion data")]
	public int Speed;

	public int Inertia;

	public int Evasion;

	public int PushPhase;

	public int FinishPhase;

	[Header("Drive explosion")]
	[SerializeField]
	private BlueprintStarshipWeaponReference m_ExplosionWeapon;

	[SerializeField]
	private BlueprintStarshipAmmoReference m_ExplosionAmmo;

	public BlueprintStarshipWeapon ExplosionWeapon => m_ExplosionWeapon?.Get();

	public BlueprintStarshipAmmo ExplosionAmmo => m_ExplosionAmmo?.Get();

	public override ItemsItemType ItemType => ItemsItemType.StarshipPlasmaDrives;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
