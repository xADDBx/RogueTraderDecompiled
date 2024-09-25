using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.Pathfinding;

public static class FiringArcHelper
{
	private readonly struct PathElement : IEquatable<PathElement>, IComparable<PathElement>
	{
		public readonly int PathLength;

		public readonly CustomGridNodeBase Node;

		public PathElement(int pathLength, CustomGridNodeBase node)
		{
			PathLength = pathLength;
			Node = node;
		}

		public int CompareTo(PathElement other)
		{
			int pathLength = PathLength;
			return pathLength.CompareTo(other.PathLength);
		}

		public override bool Equals(object obj)
		{
			if (obj is PathElement other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(PathElement other)
		{
			if (PathLength == other.PathLength)
			{
				return EqualityComparer<CustomGridNodeBase>.Default.Equals(Node, other.Node);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = -1488550433 * -1521134295;
			int pathLength = PathLength;
			return (num + pathLength.GetHashCode()) * -1521134295 + EqualityComparer<CustomGridNodeBase>.Default.GetHashCode(Node);
		}

		public static bool operator ==(PathElement left, PathElement right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PathElement left, PathElement right)
		{
			return !(left == right);
		}

		public static bool operator <(PathElement left, PathElement right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(PathElement left, PathElement right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(PathElement left, PathElement right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(PathElement left, PathElement right)
		{
			return left.CompareTo(right) >= 0;
		}
	}

	private static readonly ThreadLocal<PriorityQueue<PathElement>> OpenList;

	private static readonly ThreadLocal<HashSet<CustomGridNodeBase>> Closed;

	private static readonly int[][] FiringSectors90;

	private static readonly int[][] FiringSectors270;

	private static readonly int[] FiringSector360;

	static FiringArcHelper()
	{
		OpenList = new ThreadLocal<PriorityQueue<PathElement>>(() => new PriorityQueue<PathElement>(Comparer<PathElement>.Default, EqualityComparer<PathElement>.Default));
		Closed = new ThreadLocal<HashSet<CustomGridNodeBase>>(() => new HashSet<CustomGridNodeBase>(100));
		FiringSector360 = Enumerable.Range(0, 8).ToArray();
		FiringSectors90 = new int[8][];
		for (int i = 0; i < 8; i++)
		{
			FiringSectors90[i] = new int[3]
			{
				CustomGraphHelper.LeftNeighbourDirection[i],
				i,
				CustomGraphHelper.RightNeighbourDirection[i]
			};
		}
		FiringSectors270 = new int[8][];
		for (int j = 0; j < 8; j++)
		{
			int num = CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[j]];
			int num2 = CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[j]];
			FiringSectors270[j] = FiringSectors90[num].Concat(FiringSectors90[j]).Concat(FiringSectors90[num2]).ToArray();
		}
	}

	public static int[] GetValidDirections(RestrictedFiringArc arc, int initialDirection)
	{
		int num = CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[initialDirection]];
		int num2 = CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[initialDirection]];
		int num3 = CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[initialDirection]]]];
		return arc switch
		{
			RestrictedFiringArc.Fore => FiringSectors90[initialDirection], 
			RestrictedFiringArc.Port => FiringSectors90[num], 
			RestrictedFiringArc.Starboard => FiringSectors90[num2], 
			RestrictedFiringArc.Aft => FiringSectors90[num3], 
			RestrictedFiringArc.Dorsal => FiringSectors270[initialDirection], 
			RestrictedFiringArc.Any => FiringSector360, 
			RestrictedFiringArc.None => Array.Empty<int>(), 
			_ => throw new NotImplementedException($"Looks like there's new firing arc: {arc}. Should be implemented here also."), 
		};
	}

	public static void TraverseGraph(CustomGridNodeBase startNode, RestrictedFiringArc arc, int startDirection, int maxLen, ICollection<CustomGridNodeBase> action)
	{
		using (ProfileScope.NewScope("TraverseGraph"))
		{
			try
			{
				if (arc == RestrictedFiringArc.Dorsal)
				{
					TraverseGraphInternal(startNode, RestrictedFiringArc.Fore, startDirection, maxLen, action);
					TraverseGraphInternal(startNode, RestrictedFiringArc.Port, startDirection, maxLen, action);
					TraverseGraphInternal(startNode, RestrictedFiringArc.Starboard, startDirection, maxLen, action);
				}
				else
				{
					TraverseGraphInternal(startNode, arc, startDirection, maxLen, action);
				}
			}
			finally
			{
				OpenList.Value.Clear();
				Closed.Value.Clear();
			}
		}
	}

	private static void TraverseGraphInternal(CustomGridNodeBase startNode, RestrictedFiringArc arc, int startDirection, int maxLen, ICollection<CustomGridNodeBase> action)
	{
		CustomGridNodeBase node = AdjustStartNode(startNode, arc, startDirection);
		int pathLength = ((arc == RestrictedFiringArc.Port || arc == RestrictedFiringArc.Starboard) ? 1 : 0);
		PriorityQueue<PathElement> value = OpenList.Value;
		value.Add(new PathElement(pathLength, node));
		HashSet<CustomGridNodeBase> value2 = Closed.Value;
		while (value.Count > 0)
		{
			PathElement pathElement = value.Dequeue();
			CustomGridNodeBase node2 = pathElement.Node;
			if (pathElement.PathLength > maxLen)
			{
				break;
			}
			if (value2.Contains(node2))
			{
				continue;
			}
			action.Add(node2);
			value2.Add(node2);
			int[] validDirections = GetValidDirections(arc, startDirection);
			foreach (int direction in validDirections)
			{
				CustomGridNodeBase neighbourAlongDirection = node2.GetNeighbourAlongDirection(direction);
				if (neighbourAlongDirection != null)
				{
					int pathLength2 = neighbourAlongDirection.CellDistanceTo(startNode);
					value.Add(new PathElement(pathLength2, neighbourAlongDirection));
				}
			}
		}
	}

	public static CustomGridNodeBase AdjustStartNode(CustomGridNodeBase startNode, RestrictedFiringArc arc, int initialDirection)
	{
		switch (arc)
		{
		case RestrictedFiringArc.Port:
		{
			int direction2 = CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[initialDirection]];
			return startNode.GetNeighbourAlongDirection(direction2);
		}
		case RestrictedFiringArc.Starboard:
		{
			int direction = CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[initialDirection]];
			return startNode.GetNeighbourAlongDirection(direction);
		}
		case RestrictedFiringArc.None:
		case RestrictedFiringArc.Any:
		case RestrictedFiringArc.Fore:
		case RestrictedFiringArc.Aft:
		case RestrictedFiringArc.Dorsal:
			return startNode;
		default:
			throw new ArgumentOutOfRangeException("arc", arc, null);
		}
	}
}
