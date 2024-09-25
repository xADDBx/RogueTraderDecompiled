using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.View.Roaming;

[KnowledgeDatabaseID("5ba5daeb53357b248b9c5891a892667f")]
public class RoamingWaypointView : EntityViewBase, ICutsceneReference
{
	public bool UseOrientation;

	public float MinIdleTime;

	public float MaxIdleTime;

	[NotNull]
	public List<CutsceneReference> IdleCutscenes = new List<CutsceneReference>();

	[NotNull]
	public List<NextWaypointEntry> NextWaypoints = new List<NextWaypointEntry>();

	[NonSerialized]
	[NotNull]
	public List<RoamingWaypointView> PrevWaypoints = new List<RoamingWaypointView>();

	public override bool CreatesDataOnLoad => true;

	public RoamingWaypointData WaypointData => base.Data as RoamingWaypointData;

	public float? GetOrientation()
	{
		if (!UseOrientation)
		{
			return null;
		}
		return base.ViewTransform.rotation.eulerAngles.y;
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new RoamingWaypointData(this));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		foreach (NextWaypointEntry nextWaypoint in NextWaypoints)
		{
			if (nextWaypoint.Waypoint != null && !nextWaypoint.Waypoint.PrevWaypoints.Contains(this))
			{
				nextWaypoint.Waypoint.PrevWaypoints.Add(this);
			}
		}
	}

	protected override void OnDrawGizmos()
	{
	}

	public bool GetUsagesFor(Cutscene cutscene)
	{
		return IdleCutscenes.HasReference(cutscene);
	}
}
