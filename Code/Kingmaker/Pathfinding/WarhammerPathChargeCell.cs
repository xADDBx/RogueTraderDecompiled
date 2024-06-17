using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathChargeCell
{
	public readonly Vector3 Position;

	public readonly int DiagonalsCount;

	public readonly float Length;

	public readonly GraphNode Node;

	public readonly GraphNode ParentNode;

	public readonly bool IsCanStand;

	public WarhammerPathChargeCell(Vector3 position, int diagonalsCount, float length, GraphNode node, GraphNode parentNode, bool isCanStand)
	{
		Position = position;
		DiagonalsCount = diagonalsCount;
		Length = length;
		Node = node;
		ParentNode = parentNode;
		IsCanStand = isCanStand;
	}

	public override string ToString()
	{
		return $"{Node} ({Position}) [Len={Length}, CanStand={IsCanStand}, DiagCount={DiagonalsCount}]";
	}
}
