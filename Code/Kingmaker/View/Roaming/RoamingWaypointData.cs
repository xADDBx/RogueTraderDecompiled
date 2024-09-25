using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[HashRoot]
public class RoamingWaypointData : SimpleEntity, IRoamingPoint, IHashable
{
	public RoamingWaypointView WaypointView => base.View as RoamingWaypointView;

	public float? Orientation => WaypointView.GetOrientation();

	protected RoamingWaypointData(JsonConstructorMark _)
		: base(_)
	{
	}

	public RoamingWaypointData(EntityViewBase view)
		: base(view)
	{
	}

	public RoamingWaypointData(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	TimeSpan IRoamingPoint.SelectIdleTime(StatefulRandom random)
	{
		return random.Range(WaypointView.MinIdleTime, WaypointView.MaxIdleTime).Seconds();
	}

	Cutscene IRoamingPoint.SelectCutscene(StatefulRandom random)
	{
		return WaypointView.IdleCutscenes.Random(random)?.Get();
	}

	IRoamingPoint IRoamingPoint.SelectNextPoint(StatefulRandom random)
	{
		return WaypointView.NextWaypoints.WeightedRandom(random)?.Waypoint?.WaypointData;
	}

	IRoamingPoint IRoamingPoint.SelectPrevPoint(StatefulRandom random)
	{
		return WaypointView.PrevWaypoints.Random(random)?.WaypointData;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
