using System;
using System.Collections.Generic;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartVision : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartVision>, IEntityPartOwner
	{
		PartVision Vision { get; }
	}

	public readonly SortedSet<BaseUnitEntity> CanBeInRange = new SortedSet<BaseUnitEntity>(Comparer<BaseUnitEntity>.Create(MechanicEntityHelper.ByIdComparison));

	[JsonProperty]
	public float? VisionRangeMetersOverride { get; set; }

	[JsonProperty]
	public float? CombatVisionRangeMetersOverride { get; set; }

	public UnitSightCache SightCache { get; private set; }

	public ScriptZone ExtendedVisionArea { get; set; }

	public float RangeMeters
	{
		get
		{
			if (base.Owner.Faction.IsPlayer)
			{
				return 22f;
			}
			float num = VisionRangeMetersOverride ?? GetDefaultVisionRange();
			if (!base.Owner.IsInCombat)
			{
				return num;
			}
			return CombatVisionRangeMetersOverride ?? Math.Max(22f, num);
		}
	}

	private float GetDefaultVisionRange()
	{
		if (!(base.Owner is StarshipEntity))
		{
			return 8.5f;
		}
		return 100f;
	}

	protected override void OnAttachOrPostLoad()
	{
		SightCache = new UnitSightCache(base.Owner);
	}

	public bool HasLOS(MechanicEntity other)
	{
		using (ProfileScope.New("PartVision.HasLOS(MechanicEntity)"))
		{
			Vector3 eyePosition = base.Owner.EyePosition;
			float rangeMeters = RangeMeters;
			if ((bool)ExtendedVisionArea && ExtendedVisionArea.Data.ContainsPosition(other.Position))
			{
				return true;
			}
			if (other is BaseUnitEntity item && !CanBeInRange.Contains(item))
			{
				return false;
			}
			if (GeometryUtils.SqrDistance2D(eyePosition, other.EyePosition) > rangeMeters * rangeMeters)
			{
				return false;
			}
			return GameHelper.CheckLOS(base.Owner, other);
		}
	}

	public bool HasLOS(GraphNode targetNode, Vector3 eyePosition)
	{
		return HasLOS((Vector3)targetNode.position, eyePosition, 0f);
	}

	private bool HasLOS(Vector3 point, Vector3 eyePosition, float fudgeRadius)
	{
		using (ProfileScope.New("PartVision.HasLos(Vector3, float)"))
		{
			float rangeMeters = RangeMeters;
			return (ExtendedVisionArea != null && ExtendedVisionArea.Data.ContainsPosition(point)) || (GeometryUtils.SqrDistance2D(eyePosition, point) <= rangeMeters * rangeMeters && !LineOfSightGeometry.Instance.HasObstacle(eyePosition, point, fudgeRadius));
		}
	}

	public bool HasLOS(EntityViewBase view)
	{
		return HasLOS(view.ViewTransform.position, base.Owner.EyePosition, (view as MapObjectView)?.FogOfWarFudgeRadius ?? 0f);
	}

	private bool HasLOSBetweenPoints(Vector3 p1, Vector3 p2, Transform t = null)
	{
		float rangeMeters = RangeMeters;
		bool num = GeometryUtils.SqrDistance2D(p1, p2) <= rangeMeters * rangeMeters;
		bool flag = LineOfSightGeometry.Instance.HasObstacle(p1, p2, t?.GetInstanceID() ?? 0);
		bool flag2 = (object)ExtendedVisionArea != null && ExtendedVisionArea.Data.ContainsPosition(p2);
		return (num && !flag) || flag2;
	}

	public bool HasLOS(GraphNode targetNode, GraphNode overridePositionNode)
	{
		return HasLOS((Vector3)targetNode.position, (Vector3)overridePositionNode.position);
	}

	public bool HasLOS(Vector3 point, Vector3 overridePosition)
	{
		try
		{
			Vector3 p = overridePosition + LosCalculations.EyeShift;
			return HasLOSBetweenPoints(p, point);
		}
		finally
		{
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (VisionRangeMetersOverride.HasValue)
		{
			float val2 = VisionRangeMetersOverride.Value;
			result.Append(ref val2);
		}
		if (CombatVisionRangeMetersOverride.HasValue)
		{
			float val3 = CombatVisionRangeMetersOverride.Value;
			result.Append(ref val3);
		}
		return result;
	}
}
