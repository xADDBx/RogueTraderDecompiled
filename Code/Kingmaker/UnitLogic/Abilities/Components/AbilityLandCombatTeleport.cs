using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("8de79e536f4d4955ada731479ecfa196")]
public class AbilityLandCombatTeleport : AbilityCustomLogic, IAbilityTargetRestriction
{
	protected static readonly TimeSpan MaxTeleportationDuration = 2.5f.Seconds();

	public TeleportationType TeleportationType;

	[ConditionalShow("IsMoveSelf")]
	[Tooltip("Радиус в котором мы ищем позицию для побега или врага для телепортации к нему.")]
	public int Range;

	[ConditionalShow("IsMoveSelf")]
	[Tooltip("Если галочка стоит, то телепортироваться будет только к видимым врагам или к видимым клеткам.")]
	public bool UseLos;

	[ConditionalShow("IsMoveSelf")]
	[Tooltip("Если галочка стоит, то пытаемся телепортироваться как можно дальше от всех врагов. В противном случае вымираем кого-то из врагов и телепортируемся рядом с ним.")]
	public bool Escape;

	[ConditionalShow("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("Выбираем наиболее удаленного от кастера врага, но с учетом приоритета и дальности. В противном случае - ближайшего.")]
	public bool SearchFurthestEnemy;

	[ConditionalShow("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("Выбрав цель пытаемся телепортироваться рядом с ней, но как можно дальше от стартовой точки. В противном случае - как можно ближе.")]
	public bool TryJumpOverEnemy;

	[ShowIf("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("По возможности пытаемся телепортироваться к врагам, подпадающим под эти условия")]
	public PropertyCalculator[] EnemyPriorityConditions;

	[Space(4f)]
	public BlueprintAbilityAreaEffectReference AreaEffectToStayAwayFrom;

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

	[ConditionalShow("IsMoveTarget")]
	public ActionList ActionsOnTargetAfter;

	private bool IsMoveSelf => TeleportationType == TeleportationType.MoveSelf;

	private bool IsMoveTarget => TeleportationType == TeleportationType.MoveTarget;

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
			PFLog.Default.Error("teleportation caster unit is missing");
			yield break;
		}
		BaseUnitEntity entityToMove = caster;
		if (IsMoveTarget)
		{
			if (target.Entity == null || target.Entity == caster)
			{
				PFLog.Default.Error("teleportation missing target unit");
				yield break;
			}
			entityToMove = target.Entity as BaseUnitEntity;
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
		teleportSettings.Targets = new List<BaseUnitEntity> { entityToMove };
		teleportSettings.LookAtPoint = GetLookAtPoint(entityToMove, IsMoveSelf ? caster.Position : target.Point);
		teleportSettings.RelaxPoints = true;
		TeleportSettings settings = teleportSettings;
		if (IsMoveSelf)
		{
			IEnumerable<BaseUnitEntity> enemies = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => caster.IsEnemy(unit) && unit.IsConscious);
			if (UseLos)
			{
				enemies = enemies.Where(delegate(BaseUnitEntity enemy)
				{
					LosCalculations.CoverType coverType2 = LosCalculations.GetWarhammerLos(caster, enemy).CoverType;
					return coverType2 == LosCalculations.CoverType.Half || coverType2 == LosCalculations.CoverType.None;
				});
			}
			PartUnitBrain brainOptional = context.Caster.GetBrainOptional();
			IEnumerable<BaseUnitEntity> source = enemies;
			if (brainOptional != null && brainOptional.IsAIEnabled)
			{
				IEnumerable<TargetInfo> enumerable = enemies.Select(delegate(BaseUnitEntity e)
				{
					TargetInfo targetInfo = new TargetInfo();
					targetInfo.Init(e);
					return targetInfo;
				});
				List<TargetInfo> hatedTargets = brainOptional.GetHatedTargets(enumerable.ToList());
				if (hatedTargets.Count > 0)
				{
					source = hatedTargets.Select((TargetInfo t) => (BaseUnitEntity)t.Entity);
				}
				foreach (TargetInfo item in enumerable)
				{
					item.Release();
				}
			}
			ILookup<bool, BaseUnitEntity> lookup = source.ToLookup((BaseUnitEntity enemy) => EnemyPriorityConditions.Any((PropertyCalculator condition) => condition.GetBoolValue(new PropertyContext(context.Ability, enemy))));
			IEnumerable<BaseUnitEntity> enumerable2 = lookup[true];
			IEnumerable<BaseUnitEntity> source2 = lookup[false];
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)caster.CurrentNode.node;
			target = null;
			if (!Escape)
			{
				if (TryFindEnemyToLandAround(caster, enumerable2.Where((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy) <= Range), out var validEnemy, out var landingNode))
				{
					target = new TargetWrapper(((CustomGridGraph)customGridNodeBase.Graph).GetNode(landingNode.x, landingNode.y).Vector3Position);
				}
				else if (TryFindEnemyToLandAround(caster, source2.Where((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy) <= Range), out validEnemy, out landingNode))
				{
					target = new TargetWrapper(((CustomGridGraph)customGridNodeBase.Graph).GetNode(landingNode.x, landingNode.y).Vector3Position);
				}
			}
			if (target == null)
			{
				HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
				if (!enumerable2.Empty())
				{
					enemies = enumerable2;
				}
				GridPatterns.AddCircleNodes(hashSet, Range, caster.Size);
				HashSet<GraphNode> hashSet2 = TempHashSet.Get<GraphNode>();
				foreach (Vector2Int item2 in hashSet)
				{
					if (!CanMoveByVector(caster, item2))
					{
						continue;
					}
					int xCoordinateInGrid = customGridNodeBase.XCoordinateInGrid;
					int zCoordinateInGrid = customGridNodeBase.ZCoordinateInGrid;
					CustomGridNodeBase node2 = ((CustomGridGraph)customGridNodeBase.Graph).GetNode(xCoordinateInGrid + item2.x, zCoordinateInGrid + item2.y);
					if (UseLos)
					{
						LosCalculations.CoverType coverType = LosCalculations.GetWarhammerLos(caster, node2, caster.SizeRect).CoverType;
						if (coverType == LosCalculations.CoverType.Full || coverType == LosCalculations.CoverType.Invisible)
						{
							continue;
						}
					}
					hashSet2.Add(((CustomGridGraph)customGridNodeBase.Graph).GetNode(xCoordinateInGrid + item2.x, zCoordinateInGrid + item2.y));
				}
				GraphNode graphNode = (Escape ? hashSet2.MaxBy((GraphNode node) => DistanceToClosestEnemyInCells((CustomGridNodeBase)node, caster.SizeRect, enemies)) : hashSet2.MinBy((GraphNode node) => DistanceToClosestEnemyInCells((CustomGridNodeBase)node, caster.SizeRect, enemies)));
				target = new TargetWrapper(graphNode.Vector3Position);
			}
		}
		if (IsMoveTarget)
		{
			if (!TryFindLandingNodeAroundUnit(entityToMove, caster, out var landingNode2))
			{
				PFLog.Default.Error("teleportation of target with no space for it");
				yield break;
			}
			CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)caster.CurrentNode.node;
			target = new TargetWrapper(((CustomGridGraph)customGridNodeBase2.Graph).GetNode(landingNode2.x, landingNode2.y).Vector3Position);
		}
		IEnumerator<AbilityDeliveryTarget> deliver = Deliver(context, settings, entityToMove, target);
		while (deliver.MoveNext())
		{
			yield return deliver.Current;
		}
		using (context.GetDataScope(caster.ToITargetWrapper()))
		{
			ActionsOnCasterAfter.Run();
		}
		if (TeleportationType != TeleportationType.MoveTarget || entityToMove == null)
		{
			yield break;
		}
		using (context.GetDataScope(entityToMove))
		{
			ActionsOnTargetAfter.Run();
		}
	}

	private static IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TeleportSettings settings, BaseUnitEntity caster, TargetWrapper target)
	{
		List<BaseUnitEntity> targets = settings.Targets;
		Vector3 point = target.Point;
		Span<Vector3> pointsArray = stackalloc Vector3[targets.Count];
		Span<float> radiusArray = stackalloc float[targets.Count];
		for (int i = 0; i < targets.Count; i++)
		{
			pointsArray[i] = (Vector3)ObstacleAnalyzer.GetNearestNode(targets[i].Position - caster.Position + point).node.position;
			radiusArray[i] = targets[i].Corpulence;
		}
		if (settings.RelaxPoints)
		{
			FreePlaceSelector.RelaxPoints(pointsArray, radiusArray, targets.Count);
		}
		Vector3 vector = pointsArray[0];
		GameObject gameObject = FxHelper.SpawnFxOnEntity(settings.PortalFromPrefab, caster.View);
		GameObject gameObject2 = FxHelper.SpawnFxOnEntity(settings.PortalToPrefab, caster.View);
		if (gameObject2 != null)
		{
			gameObject2.transform.position = vector;
		}
		Vector3 value = ObjectExtensions.Or(ObjectExtensions.Or(gameObject, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? caster.Position;
		Vector3 value2 = ObjectExtensions.Or(ObjectExtensions.Or(gameObject2, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? vector;
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

	private bool CanMoveByVector(MechanicEntity caster, Vector2Int movement)
	{
		BlueprintAbilityAreaEffect areaEffect = AreaEffectToStayAwayFrom?.Get();
		CustomGridNodeBase node;
		foreach (CustomGridNodeBase occupiedNode in caster.GetOccupiedNodes())
		{
			int xCoordinateInGrid = occupiedNode.XCoordinateInGrid;
			int zCoordinateInGrid = occupiedNode.ZCoordinateInGrid;
			CustomGridGraph customGridGraph = (CustomGridGraph)occupiedNode.Graph;
			node = customGridGraph.GetNode(xCoordinateInGrid + movement.x, zCoordinateInGrid + movement.y);
			if (!UsableForLanding(node))
			{
				return false;
			}
		}
		return true;
		bool UsableForLanding(CustomGridNodeBase landingNode)
		{
			if (landingNode.Walkable && (!node.TryGetUnit(out var unit) || !unit.IsConscious || unit == caster))
			{
				if (areaEffect != null)
				{
					return !Game.Instance.State.AreaEffects.All.Any((AreaEffectEntity area) => area.IsInGame && area.Blueprint == areaEffect && area.Contains(landingNode));
				}
				return true;
			}
			return false;
		}
	}

	private bool CanCoverNode(BaseUnitEntity caster, Vector2Int nodeToCover, out Vector2Int nodeToLand)
	{
		foreach (CustomGridNodeBase occupiedNode in caster.GetOccupiedNodes())
		{
			int xCoordinateInGrid = ((CustomGridNodeBase)caster.CurrentNode.node).XCoordinateInGrid;
			int zCoordinateInGrid = ((CustomGridNodeBase)caster.CurrentNode.node).ZCoordinateInGrid;
			if (CanMoveByVector(caster, nodeToCover - occupiedNode.CoordinatesInGrid))
			{
				nodeToLand = nodeToCover + new Vector2Int(xCoordinateInGrid, zCoordinateInGrid) - occupiedNode.CoordinatesInGrid;
				return true;
			}
		}
		nodeToLand = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	private bool TryFindLandingNodeAroundUnit(BaseUnitEntity caster, BaseUnitEntity targetUnit, out Vector2Int landingNode)
	{
		if (!TryJumpOverEnemy && caster.DistanceToInCells(targetUnit) <= 1)
		{
			landingNode = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
			return true;
		}
		IEnumerable<CustomGridNodeBase> source = from node in GridAreaHelper.GetNodesSpiralAround((CustomGridNodeBase)targetUnit.CurrentNode.node, targetUnit.SizeRect, 1)
			where node.Walkable
			select node;
		source = (TryJumpOverEnemy ? source.OrderByDescending((CustomGridNodeBase node) => caster.DistanceTo(node.Vector3Position)) : source.OrderBy((CustomGridNodeBase node) => caster.DistanceTo(node.Vector3Position)));
		foreach (CustomGridNodeBase item in source)
		{
			if (CanCoverNode(caster, new Vector2Int(item.XCoordinateInGrid, item.ZCoordinateInGrid), out landingNode))
			{
				return true;
			}
		}
		landingNode = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	private bool TryFindEnemyToLandAround(BaseUnitEntity caster, IEnumerable<BaseUnitEntity> enemies, out BaseUnitEntity validEnemy, out Vector2Int landingNode)
	{
		enemies = ((!SearchFurthestEnemy) ? enemies.OrderByDescending((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy)) : enemies.OrderBy((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy)));
		foreach (BaseUnitEntity enemy in enemies)
		{
			if (TryFindLandingNodeAroundUnit(caster, enemy, out landingNode))
			{
				validEnemy = enemy;
				return true;
			}
		}
		validEnemy = null;
		landingNode = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string unavailabilityReason;
		return IsValid(ability, target, out unavailabilityReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		IsValid(ability, target, out var unavailabilityReason);
		return unavailabilityReason;
	}

	private bool IsValid(AbilityData ability, TargetWrapper target, out string unavailabilityReason)
	{
		unavailabilityReason = null;
		switch (TeleportationType)
		{
		case TeleportationType.MoveSelf:
			return true;
		case TeleportationType.MoveTarget:
		{
			if (!target.HasEntity)
			{
				unavailabilityReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
				return false;
			}
			if (!TryFindLandingNodeAroundUnit(target.Entity as BaseUnitEntity, ability.Caster as BaseUnitEntity, out var _))
			{
				unavailabilityReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.NotEnoughSpace;
				return false;
			}
			return true;
		}
		default:
			PFLog.Default.Error("unknown teleportation type");
			return true;
		}
	}
}
