using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public interface INodeLink
{
	float CostFactor { get; }

	Vector3 EndToStartDirection { get; }

	Bounds Bounds { get; }

	Vector3 GetConnectionPosition(ILinkTraversalProvider warhammerNodeLinkTraverser);

	bool CanStartTraverse(ILinkTraversalProvider traverser);

	bool IsInTraverse();

	bool IsInTraverse(ILinkTraversalProvider warhammerNodeLinkTraverser);

	void StartTransition(ILinkTraversalProvider warhammerNodeLinkTraverser);

	void CompleteTransition(ILinkTraversalProvider warhammerNodeLinkTraverser);

	bool ConnectsNodes(GraphNode from, GraphNode to);

	float GetHeight();

	bool IsCorrectSize(ILinkTraversalProvider traverser);
}
