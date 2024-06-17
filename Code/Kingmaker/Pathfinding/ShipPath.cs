using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[MemoryPackable(GenerateType.Object)]
public class ShipPath : Path, IPathBlockModeOwner, IMemoryPackable<ShipPath>, IMemoryPackFormatterRegister
{
	public struct PathCell
	{
		public int Direction;

		public int[] ParentDirections;

		public int StraightDistance;

		public int DiagonalsCount;

		public int Length;

		public bool CanStand;
	}

	public enum TurnAngleType
	{
		Turn45,
		Turn90,
		Turn135,
		Turn180
	}

	public class DirectionalPathNode : IComparable<DirectionalPathNode>
	{
		public ushort pathID;

		public CustomGridNodeBase node;

		public int direction;

		public DirectionalPathNode parent;

		public int lengthFromStart;

		public int straightDistance;

		public int diagonalCount;

		public int turnCount;

		public bool canStand;

		public bool IsDiagonal => direction > 3;

		public void Reparent(DirectionalPathNode newParent)
		{
			parent = newParent;
			diagonalCount = newParent.diagonalCount + (IsDiagonal ? 1 : 0);
			int num = ((diagonalCount % 2 != 0 || !IsDiagonal) ? 1 : 2);
			lengthFromStart = newParent.lengthFromStart + num;
			if (newParent.direction == direction)
			{
				straightDistance = newParent.straightDistance + num;
				turnCount = newParent.turnCount;
			}
			else
			{
				straightDistance = num;
				turnCount = newParent.turnCount + 1;
			}
		}

		public int CompareTo(DirectionalPathNode b)
		{
			int num = lengthFromStart - b.lengthFromStart;
			if (num != 0)
			{
				return num;
			}
			int num2 = turnCount;
			int num3 = b.turnCount;
			int num4 = num2 - num3;
			if (num4 != 0)
			{
				return num4;
			}
			int num5 = straightDistance;
			int num6 = b.straightDistance;
			return -(num5 - num6);
		}

		public static bool operator <(DirectionalPathNode left, DirectionalPathNode right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(DirectionalPathNode left, DirectionalPathNode right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(DirectionalPathNode left, DirectionalPathNode right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(DirectionalPathNode left, DirectionalPathNode right)
		{
			return left.CompareTo(right) >= 0;
		}

		public override string ToString()
		{
			return $"DirectionalPathNode: ({node.XCoordinateInGrid}, {node.ZCoordinateInGrid}), {GetDirString(direction)}";
		}

		private string GetDirString(int dir)
		{
			return dir switch
			{
				0 => "down", 
				1 => "right", 
				3 => "left", 
				4 => "down-right", 
				5 => "up-right", 
				6 => "up-left", 
				7 => "down-left", 
				_ => "up", 
			};
		}
	}

	[Preserve]
	private sealed class ShipPathFormatter : MemoryPackFormatter<ShipPath>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ShipPath value)
		{
			ShipPath.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ShipPath value)
		{
			ShipPath.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackIgnore]
	public int PathNodeSize;

	[MemoryPackIgnore]
	public PathEndingCondition endingCondition;

	internal int searchedNodes;

	private int Direction;

	private int Speed;

	private int StraightDistanceToTurn;

	private int InitialStraightDistance;

	private int InitialDiagonalsCount;

	private TurnAngleType[] TurnAngles;

	private DirectionalPathNode Current;

	private static readonly ThreadLocal<Dictionary<int, DirectionalPathNode[]>> Cache;

	private static readonly ThreadLocal<HashSet<DirectionalPathNode>> FinalPositions;

	private static readonly ThreadLocal<HashSet<DirectionalPathNode>> OpenList;

	private static readonly ThreadLocal<DirectionalPathNode> TempNode;

	[MemoryPackIgnore]
	public Dictionary<GraphNode, PathCell> Result { get; private set; }

	[MemoryPackIgnore]
	public HashSet<DirectionalPathNode> RawResult { get; private set; }

	[MemoryPackIgnore]
	public CustomGridNode startNode { get; private set; }

	[MemoryPackIgnore]
	public Vector3 startPoint { get; private set; }

	[MemoryPackIgnore]
	public Vector3 originalStartPoint { get; private set; }

	[MemoryPackIgnore]
	public int MaxLength => Speed;

	public BlockMode PathBlockMode => BlockMode.AllExceptSelector;

	public static ShipPath Construct(Vector3 start, int direction, int initialStraightDistance, int initialDiagonalsCount, int straightDistanceToTurn, int speed, IntRect shipSizeRect = default(IntRect), TurnAngleType[] turnAngles = null, OnPathDelegate callback = null)
	{
		ShipPath shipPath = PathPool.GetPath<ShipPath>();
		shipPath.Setup(start, direction, initialStraightDistance, initialDiagonalsCount, straightDistanceToTurn, speed, shipSizeRect, turnAngles, callback);
		return shipPath;
	}

	internal void FailWithError(string msg)
	{
		Error();
	}

	protected void Setup(Vector3 start, int direction, int initialStraightDistance, int initialDiagonalsCount, int straightDistanceToTurn, int speed, IntRect shipSizeRect, TurnAngleType[] turnAngles, OnPathDelegate callback)
	{
		base.callback = callback;
		startPoint = start;
		originalStartPoint = startPoint;
		Direction = direction;
		InitialDiagonalsCount = initialDiagonalsCount;
		InitialStraightDistance = initialStraightDistance;
		StraightDistanceToTurn = straightDistanceToTurn;
		Speed = speed;
		PathNodeSize = shipSizeRect.Width;
		TurnAngles = turnAngles ?? new TurnAngleType[0];
	}

	protected override void Reset()
	{
		base.Reset();
		originalStartPoint = Vector3.zero;
		startPoint = Vector3.zero;
		startNode = null;
		heuristic = Heuristic.None;
		Direction = 0;
		InitialDiagonalsCount = 0;
		InitialStraightDistance = 0;
		StraightDistanceToTurn = 0;
		Speed = 0;
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		nnConstraint.Reset();
	}

	protected override void Prepare()
	{
		nnConstraint.tags = enabledTags;
		startNode = AstarPath.active.GetNearest(startPoint, nnConstraint).node as CustomGridNode;
		if (startNode == null)
		{
			FailWithError("Could not find close node to the start point");
		}
	}

	protected override void Initialize()
	{
		OpenList.Value.Clear();
		Cache.Value.Clear();
		FinalPositions.Value.Clear();
		DirectionalPathNode pathNode = GetPathNode(startNode, Direction);
		pathNode.node = startNode;
		pathNode.pathID = pathHandler.PathID;
		pathNode.parent = null;
		pathNode.lengthFromStart = 0;
		pathNode.straightDistance = InitialStraightDistance;
		pathNode.turnCount = 0;
		pathNode.diagonalCount = InitialDiagonalsCount;
		pathNode.canStand = true;
		Open(pathNode);
		searchedNodes++;
		PlaceNode(pathNode);
		if (OpenList.Value.Count == 0)
		{
			base.CompleteState = PathCompleteState.Complete;
		}
		else
		{
			Current = RemoveOpen();
		}
	}

	private (GraphNode node, PathCell cell) Convert(DirectionalPathNode main, DirectionalPathNode[] all)
	{
		return (node: main.node, cell: new PathCell
		{
			Direction = main.direction,
			ParentDirections = (all?.Select((DirectionalPathNode v) => (v?.parent == null) ? (-1) : v.parent.direction).ToArray() ?? Array.Empty<int>()),
			StraightDistance = main.straightDistance,
			DiagonalsCount = main.diagonalCount,
			Length = main.lengthFromStart,
			CanStand = main.canStand
		});
	}

	protected override void Cleanup()
	{
		RawResult = new HashSet<DirectionalPathNode>(FinalPositions.Value);
		Dictionary<int, DirectionalPathNode> dictionary = new Dictionary<int, DirectionalPathNode>();
		foreach (DirectionalPathNode item in RawResult)
		{
			if (!dictionary.TryGetValue(item.node.NodeIndex, out var value) || item < value)
			{
				dictionary[item.node.NodeIndex] = item;
			}
		}
		Result = dictionary.Values.Select((DirectionalPathNode v) => Convert(v, Cache.Value[v.node.NodeIndex])).ToDictionary(((GraphNode node, PathCell cell) v) => v.node, ((GraphNode node, PathCell cell) v) => v.cell);
		OpenList.Value.Clear();
		Cache.Value.Clear();
		FinalPositions.Value.Clear();
	}

	protected override void CalculateStep(long targetTick)
	{
		int num = 0;
		while (base.CompleteState == PathCompleteState.NotCalculated)
		{
			searchedNodes++;
			if (Current.lengthFromStart > Speed)
			{
				base.CompleteState = PathCompleteState.Complete;
				break;
			}
			PlaceNode(Current);
			Open(Current);
			if (OpenList.Value.Count == 0)
			{
				base.CompleteState = PathCompleteState.Complete;
				break;
			}
			Current = RemoveOpen();
			if (num > 500)
			{
				if (DateTime.UtcNow.Ticks >= targetTick)
				{
					break;
				}
				num = 0;
				if (searchedNodes > 1000000)
				{
					throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
				}
			}
			num++;
		}
	}

	private bool IsSuitableDirection(DirectionalPathNode from, int newDirection)
	{
		if (from.direction == newDirection)
		{
			return true;
		}
		if (from.straightDistance < StraightDistanceToTurn)
		{
			return false;
		}
		foreach (int neighbourDirection in GetNeighbourDirections(from))
		{
			if (newDirection == neighbourDirection)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerable<int> GetNeighbourDirections(DirectionalPathNode from)
	{
		int neighbour1 = CustomGraphHelper.LeftNeighbourDirection[from.direction];
		int neighbour2 = CustomGraphHelper.RightNeighbourDirection[from.direction];
		yield return neighbour1;
		yield return neighbour2;
		if (from.turnCount < TurnAngles.Length)
		{
			int turn = (int)TurnAngles[from.turnCount];
			for (int i = 0; i < turn; i++)
			{
				neighbour1 = CustomGraphHelper.LeftNeighbourDirection[neighbour1];
				neighbour2 = CustomGraphHelper.RightNeighbourDirection[neighbour2];
				yield return neighbour1;
				yield return neighbour2;
			}
		}
	}

	private static DirectionalPathNode GetPathNode(CustomGridNodeBase node, int direction)
	{
		if (!Cache.Value.TryGetValue(node.NodeIndex, out var value))
		{
			value = new DirectionalPathNode[8];
			Cache.Value.Add(node.NodeIndex, value);
		}
		DirectionalPathNode directionalPathNode = value[direction];
		if (directionalPathNode == null)
		{
			directionalPathNode = new DirectionalPathNode();
			directionalPathNode.node = node;
			directionalPathNode.direction = direction;
			value[direction] = directionalPathNode;
		}
		return directionalPathNode;
	}

	private static void PlaceNode(DirectionalPathNode node)
	{
		FinalPositions.Value.Add(node);
	}

	private void AddOrUpdateOpen(DirectionalPathNode node)
	{
		if (!OpenList.Value.Contains(node))
		{
			OpenList.Value.Add(node);
		}
	}

	private DirectionalPathNode RemoveOpen()
	{
		DirectionalPathNode directionalPathNode = OpenList.Value.OrderBy((DirectionalPathNode v) => v).FirstOrDefault();
		if (directionalPathNode.node != null)
		{
			OpenList.Value.Remove(directionalPathNode);
		}
		return directionalPathNode;
	}

	private void Open(DirectionalPathNode pathNode)
	{
		CustomGridNodeBase node = pathNode.node;
		ushort num = pathHandler.PathID;
		for (int i = 0; i < 8; i++)
		{
			CustomGridNodeBase nodeAlongDirection = GetNodeAlongDirection(node, i);
			if (nodeAlongDirection == null || !((IWarhammerTraversalProvider)traversalProvider).CanTraverseAlongDirection(this, node, i) || !IsSuitableDirection(pathNode, i))
			{
				continue;
			}
			DirectionalPathNode pathNode2 = GetPathNode(nodeAlongDirection, i);
			pathNode2.canStand = ((IWarhammerTraversalProvider)traversalProvider).CanTraverseEndNode(pathNode2.node, pathNode2.direction);
			if (pathNode2.pathID != num)
			{
				pathNode2.pathID = num;
				pathNode2.Reparent(pathNode);
				AddOrUpdateOpen(pathNode2);
				continue;
			}
			TempNode.Value.pathID = num;
			TempNode.Value.node = nodeAlongDirection;
			TempNode.Value.direction = i;
			TempNode.Value.Reparent(pathNode);
			if (TempNode.Value < pathNode2)
			{
				pathNode2.Reparent(pathNode);
				UpdateRecursiveG(pathNode2);
			}
		}
	}

	private CustomGridNodeBase GetNodeAlongDirection(CustomGridNodeBase sourceNode, int direction)
	{
		CustomGridNodeBase customGridNodeBase = sourceNode;
		for (int i = 0; i < PathNodeSize; i++)
		{
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(direction);
		}
		return customGridNodeBase;
	}

	private void UpdateRecursiveG(DirectionalPathNode pathNode)
	{
		pathNode.Reparent(pathNode.parent);
		AddOrUpdateOpen(pathNode);
		ushort num = pathHandler.PathID;
		for (int i = 0; i < 8; i++)
		{
			CustomGridNodeBase neighbourAlongDirection = pathNode.node.GetNeighbourAlongDirection(i);
			if (neighbourAlongDirection != null)
			{
				DirectionalPathNode pathNode2 = GetPathNode(neighbourAlongDirection, i);
				if (pathNode2.parent == pathNode && pathNode2.pathID == num)
				{
					UpdateRecursiveG(pathNode2);
				}
			}
		}
	}

	static ShipPath()
	{
		Cache = new ThreadLocal<Dictionary<int, DirectionalPathNode[]>>(() => new Dictionary<int, DirectionalPathNode[]>());
		FinalPositions = new ThreadLocal<HashSet<DirectionalPathNode>>(() => new HashSet<DirectionalPathNode>());
		OpenList = new ThreadLocal<HashSet<DirectionalPathNode>>(() => new HashSet<DirectionalPathNode>());
		TempNode = new ThreadLocal<DirectionalPathNode>(() => new DirectionalPathNode());
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ShipPath>())
		{
			MemoryPackFormatterProvider.Register(new ShipPathFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ShipPath[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ShipPath>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Vector3>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PathCompleteState>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PathCompleteState>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlockMode>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<BlockMode>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ShipPath? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(5);
		writer.WriteValue(in value.vectorPath);
		ref long value2 = ref value.pathRequestedTick;
		ref bool value3 = ref value.persistentPath;
		PathCompleteState value4 = value.CompleteState;
		BlockMode value5 = value.PathBlockMode;
		writer.WriteUnmanaged(in value2, in value3, in value4, in value5);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ShipPath? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<Vector3> value2;
		long value3;
		bool value4;
		PathCompleteState value5;
		if (memberCount == 5)
		{
			BlockMode value6;
			if (value != null)
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
				value6 = value.PathBlockMode;
				reader.ReadValue(ref value2);
				reader.ReadUnmanaged<long>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<PathCompleteState>(out value5);
				reader.ReadUnmanaged<BlockMode>(out value6);
				goto IL_011d;
			}
			value2 = reader.ReadValue<List<Vector3>>();
			reader.ReadUnmanaged<long, bool, PathCompleteState, BlockMode>(out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ShipPath), 5, memberCount);
				return;
			}
			BlockMode value6;
			if (value == null)
			{
				value2 = null;
				value3 = 0L;
				value4 = false;
				value5 = PathCompleteState.NotCalculated;
				value6 = BlockMode.AllExceptSelector;
			}
			else
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
				value6 = value.PathBlockMode;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<long>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<PathCompleteState>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<BlockMode>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_011d;
			}
		}
		value = new ShipPath
		{
			vectorPath = value2,
			pathRequestedTick = value3,
			persistentPath = value4,
			CompleteState = value5
		};
		return;
		IL_011d:
		value.vectorPath = value2;
		value.pathRequestedTick = value3;
		value.persistentPath = value4;
		value.CompleteState = value5;
	}
}
