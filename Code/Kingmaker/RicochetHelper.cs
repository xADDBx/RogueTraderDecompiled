using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker;

public static class RicochetHelper
{
	public class RicochetTargetData
	{
		public MechanicEntity RicochetTargetEntity { get; }

		public Vector3 RicochetFromPoint { get; }

		public CustomGridNodeBase RicochetFromNode { get; }

		public RicochetTargetData(MechanicEntity entity, Vector3 point, CustomGridNodeBase node)
		{
			RicochetTargetEntity = entity;
			RicochetFromPoint = point;
			RicochetFromNode = node;
		}
	}

	public readonly struct RicochetTargetingParameters
	{
		public MechanicEntity LastHitEntity { get; }

		public CustomGridNodeBase LastHitNode { get; }

		public DamageData OverpenetrationData { get; }

		public AbilityData Ability { get; }

		[CanBeNull]
		public HashSet<MechanicEntity> HitTargets { get; }

		[CanBeNull]
		public Vector3? PreferredRicochetPoint { get; }

		public RicochetTargetingParameters(MechanicEntity lastHitEntity, CustomGridNodeBase lastHitNode, DamageData overpenetrationData, AbilityData ability)
		{
			LastHitEntity = lastHitEntity;
			LastHitNode = lastHitNode;
			OverpenetrationData = overpenetrationData;
			Ability = ability;
			HitTargets = null;
			PreferredRicochetPoint = null;
		}

		public RicochetTargetingParameters(MechanicEntity lastHitEntity, CustomGridNodeBase lastHitNode, DamageData overpenetrationData, AbilityData ability, HashSet<MechanicEntity> hitTargets, Vector3? preferredRicochetPoint)
		{
			LastHitEntity = lastHitEntity;
			LastHitNode = lastHitNode;
			OverpenetrationData = overpenetrationData;
			Ability = ability;
			HitTargets = hitTargets;
			PreferredRicochetPoint = preferredRicochetPoint;
		}
	}

	public static List<RicochetTargetData> GetPossibleRicochetTargets(in RicochetTargetingParameters parameters)
	{
		float radius = (float)BlueprintWarhammerRoot.Instance.CombatRoot.RicochetRange * GraphParamsMechanicsCache.GridCellSize;
		bool flag = parameters.OverpenetrationData?.IsRicochetFriendlyFireDisabled ?? false;
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(parameters.LastHitNode.Vector3Position, radius);
		List<RicochetTargetData> list2 = TempList.Get<RicochetTargetData>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			BaseUnitEntity baseUnitEntity = list[num];
			if (baseUnitEntity != parameters.LastHitEntity && !baseUnitEntity.IsInFogOfWar && baseUnitEntity.CanBeAttackedDirectly && parameters.Ability.IsValidTargetForAttack(baseUnitEntity) && (parameters.HitTargets == null || !parameters.HitTargets.Contains(baseUnitEntity)) && !(!baseUnitEntity.IsEnemy(parameters.Ability.Caster) && flag) && HasLosToUnit(baseUnitEntity, parameters.LastHitEntity, parameters.PreferredRicochetPoint, parameters.LastHitNode, out var ricochetPoint, out var ricochetNode))
			{
				list2.Add(new RicochetTargetData(baseUnitEntity, ricochetPoint ?? ricochetNode.Vector3Position, ricochetNode));
			}
		}
		return list2;
	}

	private static bool HasLosToUnit(MechanicEntity to, MechanicEntity from, [CanBeNull] Vector3? preferredRicochetPoint, [CanBeNull] CustomGridNodeBase fromNode, out Vector3? ricochetPoint, out CustomGridNodeBase ricochetNode)
	{
		ricochetPoint = null;
		ricochetNode = null;
		IntRect intRect = new IntRect(0, 0, 0, 0);
		if (preferredRicochetPoint.HasValue)
		{
			foreach (CustomGridNodeBase occupiedNode in to.GetOccupiedNodes())
			{
				if (LosCalculations.HasLos(fromNode, intRect, occupiedNode, intRect))
				{
					ricochetPoint = preferredRicochetPoint;
					ricochetNode = fromNode;
					return true;
				}
			}
		}
		if (from is UnitEntity)
		{
			foreach (CustomGridNodeBase occupiedNode2 in from.GetOccupiedNodes())
			{
				foreach (CustomGridNodeBase occupiedNode3 in to.GetOccupiedNodes())
				{
					if (LosCalculations.HasLos(occupiedNode2, intRect, occupiedNode3, intRect))
					{
						ricochetNode = occupiedNode2;
						if (!preferredRicochetPoint.HasValue)
						{
							ricochetPoint = ricochetNode.Vector3Position;
						}
						else
						{
							ricochetPoint = new Vector3(ricochetNode.Vector3Position.x, preferredRicochetPoint.Value.y, ricochetNode.Vector3Position.z);
						}
						return true;
					}
				}
			}
		}
		return false;
	}
}
