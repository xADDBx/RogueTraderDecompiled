using System;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Serialization;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class CustomGridNode : CustomGridNodeBase
{
	public readonly struct DiagonalPortalInfo
	{
		public readonly Vector3 Middle;

		public readonly Vector3 Cross;

		public readonly bool LeftClear;

		public readonly bool RightClear;

		public DiagonalPortalInfo(Vector3 middle, Vector3 cross, bool leftClear, bool rightClear)
		{
			Middle = middle;
			Cross = cross;
			LeftClear = leftClear;
			RightClear = rightClear;
		}
	}

	private static CustomGridGraph[] _gridGraphs = new CustomGridGraph[0];

	private const int GridFlagsConnectionOffset = 0;

	private const int GridFlagsConnectionBit0 = 1;

	private const int GridFlagsConnectionMask = 255;

	private const int GridFlagsEdgeNodeOffset = 10;

	private const int GridFlagsEdgeNodeMask = 1024;

	public const int GridFlagsPrimaryDirectionConnectionsMask = 15;

	internal ushort InternalGridFlags
	{
		get
		{
			return gridFlags;
		}
		set
		{
			gridFlags = value;
		}
	}

	public bool EdgeNode
	{
		get
		{
			return (gridFlags & 0x400) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFBFFu) | (value ? 1024u : 0u));
		}
	}

	public CustomGridNode(AstarPath astar)
		: base(astar)
	{
	}

	public static CustomGridGraph GetGridGraph(uint graphIndex)
	{
		return _gridGraphs[graphIndex];
	}

	public static void SetGridGraph(int graphIndex, CustomGridGraph graph)
	{
		if (_gridGraphs.Length <= graphIndex)
		{
			CustomGridGraph[] array = new CustomGridGraph[graphIndex + 1];
			for (int i = 0; i < _gridGraphs.Length; i++)
			{
				array[i] = _gridGraphs[i];
			}
			_gridGraphs = array;
		}
		_gridGraphs[graphIndex] = graph;
	}

	public override bool HasConnectionInDirection(int dir)
	{
		return ((gridFlags >> dir) & 1) != 0;
	}

	[Obsolete("Use HasConnectionInDirection")]
	public bool GetConnectionInternal(int dir)
	{
		return HasConnectionInDirection(dir);
	}

	public void SetConnectionInternal(int dir, bool value)
	{
		gridFlags = (ushort)((gridFlags & ~(1 << dir)) | ((value ? 1 : 0) << dir));
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public void SetAllConnectionInternal(int connections)
	{
		gridFlags = (ushort)((gridFlags & 0xFFFFFF00u) | (uint)connections);
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public void ResetConnectionsInternal()
	{
		gridFlags = (ushort)(gridFlags & 0xFFFFFF00u);
		AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
	}

	public override CustomGridNodeBase GetNeighbourAlongDirection(int direction, bool checkConnectivity = true)
	{
		if (HasConnectionInDirection(direction) || !checkConnectivity)
		{
			CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
			int num = base.NodeInGridIndex + gridGraph.neighbourOffsets[direction];
			int num2 = num % gridGraph.width;
			if (num >= 0 && num < gridGraph.nodes.Length && Math.Abs(num2 - base.XCoordinateInGrid) < 2)
			{
				return gridGraph.nodes[num];
			}
			return null;
		}
		return null;
	}

	public override void ClearConnections(bool alsoReverse)
	{
		if (alsoReverse)
		{
			for (int i = 0; i < 8; i++)
			{
				if (GetNeighbourAlongDirection(i) is CustomGridNode customGridNode)
				{
					customGridNode.SetConnectionInternal((i < 4) ? ((i + 2) % 4) : ((i - 2) % 4 + 4), value: false);
				}
			}
		}
		ResetConnectionsInternal();
		base.ClearConnections(alsoReverse);
	}

	public override void GetConnections(Action<GraphNode> action)
	{
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		CustomGridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 8; i++)
		{
			if (HasConnectionInDirection(i))
			{
				CustomGridNode customGridNode = nodes[base.NodeInGridIndex + neighbourOffsets[i]];
				if (customGridNode != null)
				{
					action(customGridNode);
				}
			}
		}
		base.GetConnections(action);
	}

	public Vector3 ClosestPointOnNode(Vector3 p)
	{
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		p = gridGraph.transform.InverseTransform(p);
		int num = base.NodeInGridIndex % gridGraph.width;
		int num2 = base.NodeInGridIndex / gridGraph.width;
		float y = gridGraph.transform.InverseTransform((Vector3)position).y;
		Vector3 point = new Vector3(Mathf.Clamp(p.x, num, (float)num + 1f), y, Mathf.Clamp(p.z, num2, (float)num2 + 1f));
		return gridGraph.transform.Transform(point);
	}

	public override bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
	{
		if (backwards)
		{
			return true;
		}
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		CustomGridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 4; i++)
		{
			if (HasConnectionInDirection(i) && other == nodes[base.NodeInGridIndex + neighbourOffsets[i]])
			{
				Vector3 vector = (Vector3)(position + other.position) * 0.5f;
				Vector3 vector2 = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
				vector2.Normalize();
				vector2 *= gridGraph.nodeSize * 0.5f;
				left.Add(vector - vector2);
				right.Add(vector + vector2);
				return true;
			}
		}
		DiagonalPortalInfo? diagonalPortalRich = GetDiagonalPortalRich(other, checkHasConnection: true);
		if (diagonalPortalRich.HasValue)
		{
			DiagonalPortalInfo valueOrDefault = diagonalPortalRich.GetValueOrDefault();
			left.Add(valueOrDefault.Middle - (valueOrDefault.LeftClear ? valueOrDefault.Cross : Vector3.zero));
			right.Add(valueOrDefault.Middle + (valueOrDefault.RightClear ? valueOrDefault.Cross : Vector3.zero));
			return true;
		}
		return false;
	}

	public DiagonalPortalInfo? GetDiagonalPortalRich(GraphNode other, bool checkHasConnection)
	{
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		CustomGridNode[] nodes = gridGraph.nodes;
		for (int i = 4; i < 8; i++)
		{
			if ((!HasConnectionInDirection(i) && checkHasConnection) || base.NodeInGridIndex + neighbourOffsets[i] >= nodes.Length || other != nodes[base.NodeInGridIndex + neighbourOffsets[i]])
			{
				continue;
			}
			bool rightClear = false;
			bool leftClear = false;
			if (HasConnectionInDirection(i - 4))
			{
				CustomGridNode customGridNode = nodes[base.NodeInGridIndex + neighbourOffsets[i - 4]];
				if (customGridNode.Walkable && customGridNode.HasConnectionInDirection((i - 4 + 1) % 4))
				{
					rightClear = true;
				}
			}
			if (HasConnectionInDirection((i - 4 + 1) % 4))
			{
				CustomGridNode customGridNode2 = nodes[base.NodeInGridIndex + neighbourOffsets[(i - 4 + 1) % 4]];
				if (customGridNode2.Walkable && customGridNode2.HasConnectionInDirection(i - 4))
				{
					leftClear = true;
				}
			}
			Vector3 middle = (Vector3)(position + other.position) * 0.5f;
			Vector3 cross = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
			cross.Normalize();
			cross *= gridGraph.nodeSize * 1.4142f;
			return new DiagonalPortalInfo(middle, cross, leftClear, rightClear);
		}
		return null;
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		CustomGridNode[] nodes = gridGraph.nodes;
		pathNode.UpdateG(path);
		handler.heap.Add(pathNode);
		ushort pathID = handler.PathID;
		int num = base.NodeInGridIndex;
		for (int i = 0; i < 8; i++)
		{
			if (HasConnectionInDirection(i))
			{
				CustomGridNode customGridNode = nodes[num + neighbourOffsets[i]];
				PathNode pathNode2 = handler.GetPathNode(customGridNode);
				if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
				{
					customGridNode.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
		base.UpdateRecursiveG(path, pathNode, handler);
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		CustomGridGraph gridGraph = GetGridGraph(base.GraphIndex);
		ushort pathID = handler.PathID;
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		uint[] neighbourCosts = gridGraph.neighbourCosts;
		CustomGridNode[] nodes = gridGraph.nodes;
		int num = base.NodeInGridIndex;
		for (int i = 0; i < 8; i++)
		{
			if (!HasConnectionInDirection(i))
			{
				continue;
			}
			CustomGridNode customGridNode = nodes[num + neighbourOffsets[i]];
			if (path.CanTraverse(customGridNode))
			{
				PathNode pathNode2 = handler.GetPathNode(customGridNode);
				uint num2 = neighbourCosts[i];
				if (pathNode2.pathID != pathID)
				{
					pathNode2.parent = pathNode;
					pathNode2.pathID = pathID;
					pathNode2.cost = num2;
					pathNode2.H = path.CalculateHScore(customGridNode);
					pathNode2.UpdateG(path);
					handler.heap.Add(pathNode2);
				}
				else if (pathNode.G + num2 + path.GetTraversalCost(customGridNode) < pathNode2.G)
				{
					pathNode2.cost = num2;
					pathNode2.parent = pathNode;
					customGridNode.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
		base.Open(path, pathNode, handler);
	}

	public override void SerializeNode(GraphSerializationContext ctx)
	{
		base.SerializeNode(ctx);
		ctx.SerializeInt3(position);
		ctx.writer.Write(gridFlags);
	}

	public override void DeserializeNode(GraphSerializationContext ctx)
	{
		base.DeserializeNode(ctx);
		position = ctx.DeserializeInt3();
		gridFlags = ctx.reader.ReadUInt16();
	}
}
