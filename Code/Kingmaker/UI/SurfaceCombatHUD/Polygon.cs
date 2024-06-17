using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public class Polygon
{
	public Vector4[] Vertices;

	private static Vector2Int s_LastMoveDir;

	public static Polygon CreateFromNodesList(List<Vector2> nodes, float gridCellSize)
	{
		Polygon polygon = new Polygon();
		if (nodes.Count == 0)
		{
			return polygon;
		}
		Vector2 originNode = nodes[0];
		HashSet<Vector2Int> hashSet = ConvertNodesToVectorInt(nodes, originNode, gridCellSize);
		Dictionary<Vector2Int, int> dictionary = new Dictionary<Vector2Int, int>();
		foreach (Vector2Int item in hashSet)
		{
			int x = item.x;
			int y = item.y;
			CollectVertex(hashSet, dictionary, new Vector2Int(x, y));
			CollectVertex(hashSet, dictionary, new Vector2Int(x + 1, y));
			CollectVertex(hashSet, dictionary, new Vector2Int(x + 1, y - 1));
			CollectVertex(hashSet, dictionary, new Vector2Int(x, y - 1));
		}
		if (dictionary.Count == 0)
		{
			return polygon;
		}
		List<Vector2Int> list = new List<Vector2Int>();
		int num = int.MaxValue;
		int index = 0;
		foreach (KeyValuePair<Vector2Int, int> item2 in dictionary)
		{
			for (int i = 0; i < item2.Value; i++)
			{
				list.Add(item2.Key);
			}
			if (item2.Key.x < num)
			{
				num = item2.Key.x;
				index = list.Count - 1;
			}
		}
		Vector2Int value = list[index];
		list[index] = list[0];
		list[0] = value;
		s_LastMoveDir = Vector2Int.zero;
		for (int j = 0; j < list.Count - 1; j++)
		{
			int num2 = FindNextVertex(hashSet, list, j);
			if (num2 == 0)
			{
				list.RemoveRange(j + 1, list.Count - j - 1);
				break;
			}
			if (j == num2)
			{
				PFLog.Default.Warning($"Next vertex not found for {list[j]}");
				break;
			}
			Vector2Int value2 = list[j + 1];
			list[j + 1] = list[num2];
			list[num2] = value2;
		}
		polygon.Vertices = ConvertIntVerticesToVector4(list, originNode, gridCellSize).ToArray();
		return polygon;
	}

	private static HashSet<Vector2Int> ConvertNodesToVectorInt(IEnumerable<Vector2> nodes, Vector2 originNode, float cellSize)
	{
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		if (nodes.Count() == 0)
		{
			return hashSet;
		}
		Vector2 vector = new Vector2(0.5f * cellSize, 0.5f * cellSize);
		foreach (Vector2 node in nodes)
		{
			Vector2 vector2 = node - originNode + vector;
			Vector2Int item = new Vector2Int((int)Mathf.Floor(vector2.x / cellSize), (int)Mathf.Floor(vector2.y / cellSize));
			hashSet.Add(item);
		}
		return hashSet;
	}

	private static List<Vector4> ConvertIntVerticesToVector4(IEnumerable<Vector2Int> intVertices, Vector2 originNode, float cellSize)
	{
		List<Vector4> list = new List<Vector4>();
		if (intVertices.Count() == 0)
		{
			return list;
		}
		Vector2 vector = new Vector2(-0.5f * cellSize, 0.5f * cellSize);
		foreach (Vector2Int intVertex in intVertices)
		{
			list.Add(new Vector4(originNode.x + (float)intVertex.x * cellSize + vector.x, 0f, originNode.y + (float)intVertex.y * cellSize + vector.y, 0f));
		}
		return list;
	}

	private static void CollectVertex(HashSet<Vector2Int> intNodesSet, Dictionary<Vector2Int, int> vertices, Vector2Int p)
	{
		if (vertices.ContainsKey(p))
		{
			if (IsDiagonalVertex(intNodesSet, p))
			{
				vertices[p] = 2;
			}
			else
			{
				vertices.Remove(p);
			}
		}
		else
		{
			vertices.Add(p, 1);
		}
	}

	private static bool IsDiagonalVertex(HashSet<Vector2Int> nodes, Vector2Int p)
	{
		bool flag = nodes.Contains(new Vector2Int(p.x - 1, p.y + 1));
		bool flag2 = nodes.Contains(new Vector2Int(p.x, p.y + 1));
		bool flag3 = nodes.Contains(new Vector2Int(p.x - 1, p.y));
		bool flag4 = nodes.Contains(new Vector2Int(p.x, p.y));
		if (flag == flag4 && flag2 == flag3)
		{
			return flag != flag3;
		}
		return false;
	}

	private static int FindNextVertex(HashSet<Vector2Int> nodes, List<Vector2Int> vertices, int index)
	{
		Vector2Int p = vertices[index];
		Vector2Int vector2Int = FindNextDirection(nodes, p);
		if (vector2Int == Vector2Int.zero)
		{
			return index;
		}
		s_LastMoveDir = vector2Int;
		return FindClosestAlongDirection(vertices, index, vector2Int);
	}

	private static Vector2Int FindNextDirection(HashSet<Vector2Int> nodes, Vector2Int p)
	{
		bool flag = CanMoveAlongDir(nodes, p, Vector2Int.left);
		bool flag2 = CanMoveAlongDir(nodes, p, Vector2Int.right);
		bool flag3 = CanMoveAlongDir(nodes, p, Vector2Int.up);
		bool flag4 = CanMoveAlongDir(nodes, p, Vector2Int.down);
		if (s_LastMoveDir == Vector2Int.left)
		{
			if (!flag4)
			{
				return Vector2Int.up;
			}
			return Vector2Int.down;
		}
		if (s_LastMoveDir == Vector2Int.down)
		{
			if (!flag2)
			{
				return Vector2Int.left;
			}
			return Vector2Int.right;
		}
		if (s_LastMoveDir == Vector2Int.right)
		{
			if (!flag3)
			{
				return Vector2Int.down;
			}
			return Vector2Int.up;
		}
		if (s_LastMoveDir == Vector2Int.up)
		{
			if (!flag)
			{
				return Vector2Int.right;
			}
			return Vector2Int.left;
		}
		if (!flag)
		{
			if (!flag2)
			{
				if (!flag3)
				{
					if (!flag4)
					{
						return Vector2Int.zero;
					}
					return Vector2Int.down;
				}
				return Vector2Int.up;
			}
			return Vector2Int.right;
		}
		return Vector2Int.left;
	}

	private static int FindClosestAlongDirection(List<Vector2Int> vertices, int index, Vector2Int step)
	{
		int num = index;
		float num2 = float.MaxValue;
		Vector2 vector = vertices[index];
		for (int i = index + 1; i < vertices.Count; i++)
		{
			Vector2 vector2 = vertices[i];
			if (Vector2.Dot((vector2 - vector).normalized, step) >= 0.9999f && (vector2 - vector).sqrMagnitude < num2)
			{
				num2 = (vector2 - vector).sqrMagnitude;
				num = i;
			}
		}
		if (num == index)
		{
			Vector2 vector3 = vertices[0];
			if (Vector2.Dot((vector3 - vector).normalized, step) >= 0.9999f && (vector3 - vector).sqrMagnitude < num2)
			{
				num = 0;
			}
		}
		return num;
	}

	private static bool CanMoveAlongDir(HashSet<Vector2Int> nodes, Vector2Int p, Vector2Int dir)
	{
		Vector2 vector = new Vector2((float)p.x - 0.25f, (float)p.y + 0.75f);
		Vector2Int vector2Int = new Vector2Int(dir.y, -dir.x);
		Vector2Int vector2Int2 = new Vector2Int(-dir.y, dir.x);
		int x = (int)Mathf.Floor(vector.x + 0.5f * (float)(dir.x + vector2Int.x));
		int y = (int)Mathf.Floor(vector.y + 0.5f * (float)(dir.y + vector2Int.y));
		int x2 = (int)Mathf.Floor(vector.x + 0.5f * (float)(dir.x + vector2Int2.x));
		int y2 = (int)Mathf.Floor(vector.y + 0.5f * (float)(dir.y + vector2Int2.y));
		Vector2Int item = new Vector2Int(x, y);
		Vector2Int item2 = new Vector2Int(x2, y2);
		if (nodes.Contains(item))
		{
			return !nodes.Contains(item2);
		}
		return false;
	}
}
