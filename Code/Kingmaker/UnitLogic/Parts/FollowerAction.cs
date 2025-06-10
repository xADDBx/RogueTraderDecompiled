using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public readonly struct FollowerAction
{
	public readonly Vector3 Position;

	public readonly float? Orientation;

	public readonly FollowerActionType Type;

	public FollowerAction(Vector3 position, float? orientation, FollowerActionType type)
	{
		Position = position;
		Orientation = orientation;
		Type = type;
	}
}
