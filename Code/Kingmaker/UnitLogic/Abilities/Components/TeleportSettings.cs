using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

public struct TeleportSettings
{
	public GameObject PortalFromPrefab;

	public GameObject PortalToPrefab;

	public string PortalBone;

	public GameObject CasterDisappearFx;

	public GameObject CasterAppearFx;

	public GameObject SideDisappearFx;

	public GameObject SideAppearFx;

	public float CasterDisappearDuration;

	public float CasterAppearDuration;

	public float SideDisappearDuration;

	public float SideAppearDuration;

	public BlueprintProjectile CasterDisappearProjectile;

	public BlueprintProjectile CasterAppearProjectile;

	public BlueprintProjectile CasterTeleportationProjectile;

	public BlueprintProjectile SideDisappearProjectile;

	public BlueprintProjectile SideAppearProjectile;

	public BlueprintProjectile SideTeleportationProjectile;

	public List<BaseUnitEntity> Targets;

	public Vector3 LookAtPoint;

	public bool RelaxPoints;
}
