using Kingmaker.Code.Enums.Helper;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

[DisallowMultipleComponent]
public class UnitMovementAgent : UnitMovementAgentBase
{
	public override Vector3 Position
	{
		get
		{
			if (base.Unit != null)
			{
				return SizePathfindingHelper.FromMechanicsToViewPosition(base.Unit.Data, base.Unit.Data.Position);
			}
			return base.transform.position;
		}
		set
		{
			if (base.Unit != null)
			{
				base.Unit.Data.Position = SizePathfindingHelper.FromViewToMechanicsPosition(base.Unit.Data, value);
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	protected override Vector2 GetNextWaypoint2D(int index)
	{
		if (base.Unit != null)
		{
			return SizePathfindingHelper.FromMechanicsToViewPosition(base.Unit.EntityData, base.Path.vectorPath[index]).To2D();
		}
		return base.GetNextWaypoint2D(index);
	}

	protected override bool IsDistanceCloseEnough(float distance)
	{
		return distance < 0.3f;
	}
}
