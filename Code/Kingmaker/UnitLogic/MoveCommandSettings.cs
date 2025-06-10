using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public struct MoveCommandSettings
{
	public Vector3 Destination;

	public BaseUnitEntity FollowedUnit;

	public bool IsControllerGamepad;

	public bool DisableApproachRadius;

	public bool LeaveFollowers;
}
