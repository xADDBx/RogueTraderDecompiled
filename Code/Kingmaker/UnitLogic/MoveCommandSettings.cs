using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public struct MoveCommandSettings
{
	public Vector3 Destination;

	public float? SpeedLimit;

	public BaseUnitEntity FollowedUnit;

	public bool IsControllerGamepad;
}
