using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("7e46f87c5651ee94199ec66539acb22c")]
public class AbilityCustomTeleport : AbilityCustomLogic, IAbilityTargetRestriction
{
	protected static readonly TimeSpan MaxTeleportationDuration = 2.5f.Seconds();

	public bool FindPosition;

	[ShowIf("FindPosition")]
	public int Range;

	[ShowIf("FindPosition")]
	[Tooltip("Регулирует искать для телепортацию точку ближайшую к врагам или наиболее удаленную")]
	public bool RunFromEnemies;

	[Space(4f)]
	public GameObject PortalFromPrefab;

	public GameObject PortalToPrefab;

	public string PortalBone = "";

	public GameObject CasterDisappearFx;

	public GameObject CasterAppearFx;

	public GameObject SideDisappearFx;

	public GameObject SideAppearFx;

	[SerializeField]
	private BlueprintProjectileReference m_CasterDisappearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_CasterAppearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_SideDisappearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_SideAppearProjectile;

	public bool LookAtRandomDirection;

	public ActionList ActionsOnCasterAfter;

	public BlueprintProjectile CasterDisappearProjectile => m_CasterDisappearProjectile?.Get();

	public BlueprintProjectile CasterAppearProjectile => m_CasterAppearProjectile?.Get();

	public BlueprintProjectile SideDisappearProjectile => m_SideDisappearProjectile?.Get();

	public BlueprintProjectile SideAppearProjectile => m_SideAppearProjectile?.Get();

	public override bool IsEngageUnit => true;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		BaseUnitEntity caster = context.Caster as BaseUnitEntity;
		if (caster == null)
		{
			PFLog.Default.Error("caster unit is missing");
			yield break;
		}
		TeleportSettings teleportSettings = default(TeleportSettings);
		teleportSettings.PortalFromPrefab = PortalFromPrefab;
		teleportSettings.PortalToPrefab = PortalToPrefab;
		teleportSettings.PortalBone = PortalBone;
		teleportSettings.CasterDisappearFx = CasterDisappearFx;
		teleportSettings.CasterAppearFx = CasterAppearFx;
		teleportSettings.SideDisappearFx = SideDisappearFx;
		teleportSettings.SideAppearFx = SideAppearFx;
		teleportSettings.CasterDisappearProjectile = CasterDisappearProjectile;
		teleportSettings.CasterAppearProjectile = CasterAppearProjectile;
		teleportSettings.SideDisappearProjectile = SideDisappearProjectile;
		teleportSettings.SideAppearProjectile = SideAppearProjectile;
		teleportSettings.Targets = GetTargets(caster);
		teleportSettings.LookAtPoint = GetLookAtPoint(caster, target.Point);
		teleportSettings.RelaxPoints = true;
		TeleportSettings settings = teleportSettings;
		CustomGridNodeBase node;
		if (FindPosition)
		{
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			GridPatterns.AddCircleNodes(hashSet, Range, caster.Size);
			IEnumerable<BaseUnitEntity> enemies = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => caster.IsEnemy(unit) && caster.IsConscious);
			HashSet<GraphNode> hashSet2 = TempHashSet.Get<GraphNode>();
			foreach (Vector2Int item in hashSet)
			{
				bool flag = true;
				foreach (CustomGridNodeBase occupiedNode in caster.GetOccupiedNodes())
				{
					int xCoordinateInGrid = occupiedNode.XCoordinateInGrid;
					int zCoordinateInGrid = occupiedNode.ZCoordinateInGrid;
					CustomGridGraph customGridGraph = (CustomGridGraph)occupiedNode.Graph;
					node = customGridGraph.GetNode(xCoordinateInGrid + item.x, zCoordinateInGrid + item.y);
					if (!UsableForLanding(node))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					int xCoordinateInGrid2 = ((CustomGridNodeBase)caster.CurrentNode.node).XCoordinateInGrid;
					int zCoordinateInGrid2 = ((CustomGridNodeBase)caster.CurrentNode.node).ZCoordinateInGrid;
					hashSet2.Add(((CustomGridGraph)caster.CurrentNode.node.Graph).GetNode(xCoordinateInGrid2 + item.x, zCoordinateInGrid2 + item.y));
				}
			}
			GraphNode graphNode = (RunFromEnemies ? hashSet2.MaxBy((GraphNode node) => DistanceToClosestEnemyInCells((CustomGridNodeBase)node, caster.SizeRect, enemies)) : hashSet2.MinBy((GraphNode node) => DistanceToClosestEnemyInCells((CustomGridNodeBase)node, caster.SizeRect, enemies)));
			settings.Targets.Clear();
			settings.Targets = new List<BaseUnitEntity> { caster };
			target = new TargetWrapper(graphNode.Vector3Position);
		}
		caster.Features.CantAct.Retain();
		IEnumerator<AbilityDeliveryTarget> deliver = Deliver(context, settings, caster, target);
		while (deliver.MoveNext())
		{
			yield return deliver.Current;
		}
		using (context.GetDataScope(caster.ToITargetWrapper()))
		{
			ActionsOnCasterAfter.Run();
		}
		caster.Features.CantAct.Release();
		bool UsableForLanding(CustomGridNodeBase landingNode)
		{
			if (landingNode.Walkable)
			{
				if (node.TryGetUnit(out var unit2) && unit2.IsConscious)
				{
					return unit2 == caster;
				}
				return true;
			}
			return false;
		}
	}

	private static IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TeleportSettings settings, BaseUnitEntity caster, TargetWrapper target)
	{
		List<BaseUnitEntity> targets = settings.Targets;
		Vector3 vector = target.Point;
		if (target.Entity != null)
		{
			float num = caster.Corpulence + target.Entity.Corpulence + 1f;
			vector += Quaternion.Euler(0f, target.Entity.Orientation, 0f) * Vector3.forward * num;
		}
		StarshipOverrideCustomTeleportLocation component = context.AbilityBlueprint.GetComponent<StarshipOverrideCustomTeleportLocation>();
		if (component != null)
		{
			Vector3? actionPosition = component.GetActionPosition(caster, target, caster.Position);
			if (actionPosition.HasValue)
			{
				Vector3 valueOrDefault = actionPosition.GetValueOrDefault();
				vector = valueOrDefault;
			}
		}
		Span<Vector3> pointsArray = stackalloc Vector3[targets.Count];
		Span<float> radiusArray = stackalloc float[targets.Count];
		for (int i = 0; i < targets.Count; i++)
		{
			pointsArray[i] = (Vector3)ObstacleAnalyzer.GetNearestNode(targets[i].Position - caster.Position + vector).node.position;
			radiusArray[i] = targets[i].Corpulence;
		}
		if (settings.RelaxPoints)
		{
			FreePlaceSelector.RelaxPoints(pointsArray, radiusArray, targets.Count);
		}
		Vector3 vector2 = pointsArray[0];
		GameObject gameObject = FxHelper.SpawnFxOnEntity(settings.PortalFromPrefab, caster.View);
		GameObject gameObject2 = FxHelper.SpawnFxOnEntity(settings.PortalToPrefab, caster.View);
		if (gameObject2 != null)
		{
			gameObject2.transform.position = vector2;
		}
		Vector3 value = ObjectExtensions.Or(ObjectExtensions.Or(gameObject, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? caster.Position;
		Vector3 value2 = ObjectExtensions.Or(ObjectExtensions.Or(gameObject2, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? vector2;
		List<IEnumerator> teleportationRoutines = new List<IEnumerator>();
		for (int j = 0; j < targets.Count; j++)
		{
			BaseUnitEntity baseUnitEntity = targets[j];
			Vector3 targetPosition = pointsArray[j];
			baseUnitEntity.Wake(10f);
			Vector3? intermediateFromPosition = ((gameObject != null) ? new Vector3?(value) : null);
			Vector3? intermediateToPosition = ((gameObject2 != null) ? new Vector3?(value2) : null);
			IEnumerator item = CreateTeleportationRoutine(context, settings, baseUnitEntity, target, targetPosition, intermediateFromPosition, intermediateToPosition, j == 0);
			teleportationRoutines.Add(item);
		}
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		while (teleportationRoutines.Count > 0 && Game.Instance.TimeController.GameTime - startTime < MaxTeleportationDuration)
		{
			for (int k = 0; k < teleportationRoutines.Count; k++)
			{
				if (!teleportationRoutines[k].MoveNext())
				{
					teleportationRoutines.RemoveAt(k);
					k--;
				}
			}
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target);
	}

	private static IEnumerator CreateTeleportationRoutine(AbilityExecutionContext context, TeleportSettings settings, BaseUnitEntity unit, TargetWrapper spellTarget, Vector3 targetPosition, Vector3? intermediateFromPosition, Vector3? intermediateToPosition, bool isCaster)
	{
		GameObject prefab = (isCaster ? settings.CasterDisappearFx : settings.SideDisappearFx);
		GameObject appearFx = (isCaster ? settings.CasterAppearFx : settings.SideAppearFx);
		BlueprintProjectile disappearProjectile = (isCaster ? settings.CasterDisappearProjectile : settings.SideDisappearProjectile);
		BlueprintProjectile appearProjectile = (isCaster ? settings.CasterAppearProjectile : settings.SideAppearProjectile);
		BlueprintProjectile teleportationProjectile = (isCaster ? settings.CasterTeleportationProjectile : settings.SideTeleportationProjectile);
		float appearDuration = (isCaster ? settings.CasterAppearDuration : settings.SideAppearDuration);
		float disappearDuration = (isCaster ? settings.CasterDisappearDuration : settings.SideDisappearDuration);
		appearDuration = Math.Max(appearDuration, 0.3f);
		unit.View.StopMoving();
		Buff dimensionDoorBuff = unit.Buffs.Add(BlueprintRoot.Instance.SystemMechanics.DimensionDoorBuff, MaxTeleportationDuration);
		FxHelper.SpawnFxOnEntity(prefab, unit.View);
		if (disappearDuration > 0.01f)
		{
			TimeSpan startTime = Game.Instance.TimeController.GameTime;
			while (Game.Instance.TimeController.GameTime - startTime < disappearDuration.Seconds())
			{
				yield return null;
			}
		}
		if (disappearProjectile != null && intermediateFromPosition.HasValue)
		{
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, disappearProjectile, unit, unit.Position, intermediateFromPosition.Value);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		if (teleportationProjectile != null)
		{
			Vector3 targetPosition2 = intermediateToPosition ?? targetPosition;
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, teleportationProjectile, unit, intermediateFromPosition, targetPosition2);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		unit.View.MovementAgent.Blocker.Unblock();
		unit.Position = targetPosition;
		unit.View.MovementAgent.Blocker.BlockAt(unit.Position);
		unit.ForceLookAt(settings.LookAtPoint);
		if (appearProjectile != null && intermediateToPosition.HasValue)
		{
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, appearProjectile, unit, intermediateToPosition, targetPosition);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		else
		{
			yield return null;
		}
		FxHelper.SpawnFxOnEntity(appearFx, unit.View);
		if (appearDuration > 0.01f)
		{
			TimeSpan startTime = Game.Instance.TimeController.GameTime;
			while (Game.Instance.TimeController.GameTime - startTime < appearDuration.Seconds())
			{
				yield return null;
			}
		}
		unit.Facts.Remove(dimensionDoorBuff);
	}

	private static IEnumerator CreateProjectileRoutine(AbilityExecutionContext context, BlueprintProjectile blueprint, BaseUnitEntity unit, Vector3? sourcePosition, Vector3 targetPosition)
	{
		Projectile projectile = new ProjectileLauncher(blueprint, unit, targetPosition).Ability(context.Ability).LaunchPosition(sourcePosition).Launch();
		float distance = projectile.Distance(unit.Position, targetPosition);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			yield return null;
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	protected virtual List<BaseUnitEntity> GetTargets(BaseUnitEntity caster)
	{
		return new List<BaseUnitEntity> { caster };
	}

	protected virtual Vector3 GetLookAtPoint(BaseUnitEntity caster, Vector3 targetPos)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			targetPos = (Vector3)ObstacleAnalyzer.GetNearestNode(targetPos).node.position;
		}
		if (LookAtRandomDirection)
		{
			return targetPos + Quaternion.AngleAxis(45 * caster.Random.Range(0, 8), Vector3.up) * Vector3.right;
		}
		return targetPos + caster.View.ViewTransform.forward;
	}

	private int DistanceToClosestEnemyInCells(CustomGridNodeBase checkNode, IntRect rect, IEnumerable<BaseUnitEntity> enemies)
	{
		return enemies.Where((BaseUnitEntity e) => e.IsConscious).Min((BaseUnitEntity e) => e.DistanceToInCells(checkNode.Vector3Position, rect));
	}

	bool IAbilityTargetRestriction.IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return ability.Caster.CanStandHere(target.NearestNode);
	}

	string IAbilityTargetRestriction.GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Root.Common.LocalizedTexts.Reasons.NotEnoughSpace;
	}
}
