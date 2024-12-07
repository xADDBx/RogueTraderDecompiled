using System;
using System.Linq;
using Kingmaker.Utility.Random;
using Pathfinding;
using Pathfinding.Serialization;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public abstract class CustomGridNodeBase : GraphNode
{
	public const int GridFlagsDirectionFlagOffsetS = 0;

	public const int GridFlagsDirectionFlagOffsetE = 1;

	public const int GridFlagsDirectionFlagOffsetN = 2;

	public const int GridFlagsDirectionFlagOffsetW = 3;

	public const int GridFlagsDirectionFlagOffsetSE = 4;

	public const int GridFlagsDirectionFlagOffsetNE = 5;

	public const int GridFlagsDirectionFlagOffsetNW = 6;

	public const int GridFlagsDirectionFlagOffsetSW = 7;

	private const int GridFlagsWalkableErosionOffset = 8;

	private const int GridFlagsWalkableErosionMask = 256;

	private const int GridFlagsWalkableTmpOffset = 9;

	private const int GridFlagsWalkableTmpMask = 512;

	protected const int NodeInGridIndexLayerOffset = 24;

	protected const int NodeInGridIndexMask = 16777215;

	protected int nodeInGridIndex;

	internal ushort gridFlags;

	private readonly int[] m_Fences = Enumerable.Repeat(int.MinValue, 8).ToArray();

	public CustomConnection[] connections;

	public int NodeInGridIndex
	{
		get
		{
			return nodeInGridIndex & 0xFFFFFF;
		}
		set
		{
			nodeInGridIndex = (nodeInGridIndex & -16777216) | value;
		}
	}

	public int XCoordinateInGrid => NodeInGridIndex % CustomGridNode.GetGridGraph(base.GraphIndex).width;

	public int ZCoordinateInGrid => NodeInGridIndex / CustomGridNode.GetGridGraph(base.GraphIndex).width;

	public Vector2Int CoordinatesInGrid => new Vector2Int(XCoordinateInGrid, ZCoordinateInGrid);

	public bool WalkableErosion
	{
		get
		{
			return (gridFlags & 0x100) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFEFFu) | (value ? 256u : 0u));
		}
	}

	public bool TmpWalkable
	{
		get
		{
			return (gridFlags & 0x200) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFDFFu) | (value ? 512u : 0u));
		}
	}

	protected CustomGridNodeBase(AstarPath astar)
		: base(astar)
	{
	}

	public override float SurfaceArea()
	{
		CustomGridGraph gridGraph = CustomGridNode.GetGridGraph(base.GraphIndex);
		return gridGraph.nodeSize * gridGraph.nodeSize;
	}

	public override Vector3 RandomPointOnSurface()
	{
		CustomGridGraph gridGraph = CustomGridNode.GetGridGraph(base.GraphIndex);
		Vector3 vector = gridGraph.transform.InverseTransform((Vector3)position);
		return gridGraph.transform.Transform(vector + new Vector3(PFStatefulRandom.Pathfinding.value - 0.5f, 0f, PFStatefulRandom.Pathfinding.value - 0.5f));
	}

	public override int GetGizmoHashCode()
	{
		int num = base.GetGizmoHashCode();
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				num ^= 17 * connections[i].GetHashCode();
			}
		}
		return num ^ (109 * gridFlags);
	}

	public bool ContainsPointInGraphSpace(Vector3 graphSpacePosition)
	{
		if (graphSpacePosition.x - (float)XCoordinateInGrid >= 0f && graphSpacePosition.x - (float)XCoordinateInGrid <= 1f && graphSpacePosition.z - (float)ZCoordinateInGrid >= 0f)
		{
			return graphSpacePosition.z - (float)ZCoordinateInGrid <= 1f;
		}
		return false;
	}

	public bool ContainsPoint(Vector3 p)
	{
		Vector3 graphSpacePosition = CustomGridNode.GetGridGraph(base.GraphIndex).transform.InverseTransform(p);
		return ContainsPointInGraphSpace(graphSpacePosition);
	}

	public bool ContainsPoint(Int3 p)
	{
		return ContainsPoint((Vector3)p);
	}

	public abstract CustomGridNodeBase GetNeighbourAlongDirection(int direction, bool checkConnectivity = true);

	public abstract bool HasConnectionInDirection(int dir);

	public override bool ContainsConnection(GraphNode node)
	{
		if (ContainsCustomConnection(node))
		{
			return true;
		}
		for (int i = 0; i < 8; i++)
		{
			if (node == GetNeighbourAlongDirection(i))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsCustomConnection(GraphNode node)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].Node == node)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearCustomConnections(bool alsoReverse)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].Node.RemoveConnection(this);
			}
		}
		connections = null;
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public override void ClearConnections(bool alsoReverse)
	{
		ClearCustomConnections(alsoReverse);
	}

	public override void GetConnections(Action<GraphNode> action)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				action(connections[i].Node);
			}
		}
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		ushort pathID = handler.PathID;
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode node = connections[i].Node;
			PathNode pathNode2 = handler.GetPathNode(node);
			if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
			{
				node.UpdateRecursiveG(path, pathNode2, handler);
			}
		}
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		ushort pathID = handler.PathID;
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			CustomConnection customConnection = connections[i];
			GraphNode node = customConnection.Node;
			if (path.CanTraverse(node) && (!(path is ILinkTraversePath { LinkTraversalProvider: not null } linkTraversePath) || linkTraversePath.LinkTraversalProvider.CanBuildPathThroughLink(pathNode.node, node, customConnection.Link)))
			{
				PathNode pathNode2 = handler.GetPathNode(node);
				uint cost = customConnection.Cost;
				if (pathNode2.pathID != pathID)
				{
					pathNode2.parent = pathNode;
					pathNode2.pathID = pathID;
					pathNode2.cost = cost;
					pathNode2.H = path.CalculateHScore(node);
					pathNode2.UpdateG(path);
					handler.heap.Add(pathNode2);
				}
				else if (pathNode.G + cost + path.GetTraversalCost(node) < pathNode2.G)
				{
					pathNode2.cost = cost;
					pathNode2.parent = pathNode;
					node.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
	}

	public override void AddConnection(GraphNode node, uint cost)
	{
		AddConnection(node, cost, null);
	}

	public void AddConnection(GraphNode node, uint cost, INodeLink link)
	{
		if (node == null)
		{
			throw new ArgumentNullException();
		}
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].Node == node)
				{
					connections[i].Cost = cost;
					connections[i].Link = link;
					return;
				}
			}
		}
		int num = ((connections != null) ? connections.Length : 0);
		CustomConnection[] array = new CustomConnection[num + 1];
		for (int j = 0; j < num; j++)
		{
			array[j] = connections[j];
		}
		array[num] = new CustomConnection(node, cost, byte.MaxValue, link);
		connections = array;
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public override void RemoveConnection(GraphNode node)
	{
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].Node == node)
			{
				int num = connections.Length;
				CustomConnection[] array = new CustomConnection[num - 1];
				for (int j = 0; j < i; j++)
				{
					array[j] = connections[j];
				}
				for (int k = i + 1; k < num; k++)
				{
					array[k - 1] = connections[k];
				}
				connections = array;
				AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
				break;
			}
		}
	}

	public void ResetFences()
	{
		for (int i = 0; i < m_Fences.Length; i++)
		{
			m_Fences[i] = int.MinValue;
		}
	}

	public void SetFenceAlongDirection(int direction, int fenceHeight)
	{
		if (direction < 4)
		{
			m_Fences[direction] = fenceHeight;
			SetFenceAlongDiagonalDirection((direction + 3) % 4 + 4, fenceHeight);
			SetFenceAlongDiagonalDirection(direction + 4, fenceHeight);
		}
		else
		{
			SetFenceAlongDiagonalDirection(direction, fenceHeight);
		}
	}

	private void SetFenceAlongDiagonalDirection(int direction, int fenceHeight)
	{
		if (direction >= 4)
		{
			m_Fences[direction] = Mathf.Max(fenceHeight, Mathf.Max(m_Fences[direction - 4], m_Fences[(direction - 3) % 4]));
		}
	}

	public bool HasFenceWithNode(CustomGridNodeBase node)
	{
		int fenceHeight;
		return HasFenceWithNode(node, out fenceHeight);
	}

	public bool HasFenceWithNode(CustomGridNodeBase node, out int fenceHeight)
	{
		IsConnectionCut(node, out fenceHeight);
		return fenceHeight > position.y;
	}

	public bool IsConnectionCut(CustomGridNodeBase node, out int fenceHeight)
	{
		int num = node.XCoordinateInGrid - XCoordinateInGrid;
		int num2 = node.ZCoordinateInGrid - ZCoordinateInGrid;
		int direction = ((num <= 0) ? ((num >= 0) ? ((num2 > 0) ? 2 : 0) : ((num2 > 0) ? 6 : ((num2 < 0) ? 7 : 3))) : ((num2 > 0) ? 5 : ((num2 >= 0) ? 1 : 4)));
		return IsConnectionCutAlongDirection(direction, out fenceHeight);
	}

	public bool IsConnectionCutAlongDirection(int direction, out int fenceHeight)
	{
		fenceHeight = m_Fences[direction];
		return fenceHeight > int.MinValue;
	}

	public override void SerializeReferences(GraphSerializationContext ctx)
	{
		if (connections == null)
		{
			ctx.writer.Write(-1);
			return;
		}
		ctx.writer.Write(connections.Length);
		for (int i = 0; i < connections.Length; i++)
		{
			ctx.SerializeNodeReference(connections[i].Node);
			ctx.writer.Write(connections[i].Cost);
		}
	}

	public override void DeserializeReferences(GraphSerializationContext ctx)
	{
		if (ctx.meta.version < AstarSerializer.V3_8_3)
		{
			return;
		}
		int num = ctx.reader.ReadInt32();
		if (num == -1)
		{
			connections = null;
			return;
		}
		connections = new CustomConnection[num];
		for (int i = 0; i < num; i++)
		{
			connections[i] = new CustomConnection(ctx.DeserializeNodeReference(), ctx.reader.ReadUInt32());
		}
	}

	public override string ToString()
	{
		return $"{GetType().Name} ({XCoordinateInGrid}, {ZCoordinateInGrid})";
	}
}
