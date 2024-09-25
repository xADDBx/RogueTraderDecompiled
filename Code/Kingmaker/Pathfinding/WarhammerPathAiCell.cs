using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct WarhammerPathAiCell
{
	public readonly Vector3 Position;

	public readonly int DiagonalsCount;

	public readonly float Length;

	public readonly GraphNode Node;

	public readonly GraphNode ParentNode;

	public readonly bool IsCanStand;

	public readonly int EnteredAoE;

	public readonly int StepsInsideDamagingAoE;

	public readonly int ProvokedAttacks;

	public WarhammerPathAiCell(Vector3 position, int diagonalsCount, float length, GraphNode node, GraphNode parentNode, bool isCanStand, int enteredAoE, int stepsInsideDamagingAoE, int provokedAttacks)
	{
		Position = position;
		DiagonalsCount = diagonalsCount;
		Length = length;
		Node = node;
		ParentNode = parentNode;
		IsCanStand = isCanStand;
		EnteredAoE = enteredAoE;
		StepsInsideDamagingAoE = stepsInsideDamagingAoE;
		ProvokedAttacks = provokedAttacks;
	}

	public override string ToString()
	{
		return $"{Node} ({Position}) [Len={Length}, CanStand={IsCanStand}, DiagCount={DiagonalsCount}]";
	}
}
