using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.BVH;

public class BoundingVolumeHierarchy<T> where T : class
{
	public delegate bool QueryCallback(T userData);

	public delegate float RaycastCallback(float3 from, float3 to, T userData);

	public struct Node
	{
		public AABB Bounds;

		public int Parent;

		public int NextFree;

		public int ChildA;

		public int ChildB;

		public int Height;

		public T UserData;

		public bool Moved;

		public bool IsLeaf => ChildA == BoundingVolumeHierarchy<T>.Null;

		public bool IsFree => Height < 0;
	}

	public struct NodePod
	{
		public AABB Bounds;

		public int Parent;

		public int ChildA;

		public int ChildB;

		public int UserDataIndex;
	}

	public static readonly int Null = -1;

	public static readonly float FatBoundsRadius = 0.25f;

	public static readonly float Sqrt3Inv = 1f / math.sqrt(3f);

	private Node[] m_Nodes;

	private NodePod[] m_Pods;

	private int m_NumNodes;

	private int m_FreeList;

	private int m_Root;

	private Stack<int> m_Stack;

	public int Capacity => m_Nodes.Length;

	public int Root => m_Root;

	public BoundingVolumeHierarchy()
	{
		m_Nodes = new Node[16];
		m_Pods = new NodePod[16];
		m_NumNodes = 0;
		m_FreeList = Null;
		m_Root = Null;
		m_Stack = new Stack<int>(256);
		for (int i = 0; i < m_Nodes.Length - 1; i++)
		{
			m_Nodes[i].NextFree = i + 1;
			m_Nodes[i].Height = -1;
		}
		m_Nodes[m_Nodes.Length - 1].NextFree = Null;
		m_Nodes[m_Nodes.Length - 1].Height = -1;
		m_FreeList = 0;
	}

	private int AllocateNode()
	{
		if (m_FreeList == Null)
		{
			Node[] nodes = m_Nodes;
			m_Nodes = new Node[nodes.Length * 2];
			nodes.CopyTo(m_Nodes, 0);
			for (int i = m_NumNodes; i < m_Nodes.Length - 1; i++)
			{
				m_Nodes[i].NextFree = i + 1;
				m_Nodes[i].Height = -1;
			}
			m_Nodes[m_Nodes.Length - 1].NextFree = Null;
			m_Nodes[m_Nodes.Length - 1].Height = -1;
			m_FreeList = m_NumNodes;
		}
		int freeList = m_FreeList;
		m_FreeList = m_Nodes[freeList].NextFree;
		m_Nodes[freeList].Parent = Null;
		m_Nodes[freeList].ChildA = Null;
		m_Nodes[freeList].ChildB = Null;
		m_Nodes[freeList].Height = 0;
		m_Nodes[freeList].NextFree = -1;
		m_Nodes[freeList].UserData = null;
		m_Nodes[freeList].Moved = false;
		m_NumNodes++;
		return freeList;
	}

	private void FreeNode(int node)
	{
		m_Nodes[node].NextFree = m_FreeList;
		m_Nodes[node].Height--;
		m_FreeList = node;
		m_NumNodes--;
	}

	public int CreateProxy(in AABB bounds, T userData)
	{
		int num = AllocateNode();
		m_Nodes[num].Bounds = bounds;
		m_Nodes[num].Bounds.Expand(FatBoundsRadius);
		m_Nodes[num].Height = 0;
		m_Nodes[num].UserData = userData;
		m_Nodes[num].Moved = true;
		InsertLeaf(num);
		return num;
	}

	public void DestroyProxy(int proxy)
	{
		RemoveLeaf(proxy);
		FreeNode(proxy);
	}

	public void UpdateProxy(int proxy, in AABB bounds)
	{
		if (!m_Nodes[proxy].Bounds.Contains(in bounds))
		{
			RemoveLeaf(proxy);
			m_Nodes[proxy].Bounds = bounds;
			m_Nodes[proxy].Bounds.Expand(FatBoundsRadius);
			InsertLeaf(proxy);
			m_Nodes[proxy].Moved = true;
		}
	}

	public AABB GetBounds(int proxy)
	{
		return m_Nodes[proxy].Bounds;
	}

	public bool Query(AABB bounds, QueryCallback callback = null)
	{
		m_Stack.Clear();
		m_Stack.Push(m_Root);
		bool result = false;
		while (m_Stack.Count > 0)
		{
			int num = m_Stack.Pop();
			if (num == Null)
			{
				continue;
			}
			AABB b = m_Nodes[num].Bounds;
			b.Expand(0f - FatBoundsRadius);
			if (!AABB.Intersects(in bounds, in b))
			{
				continue;
			}
			if (m_Nodes[num].IsLeaf)
			{
				result = true;
				if (callback != null && !callback(m_Nodes[num].UserData))
				{
					return true;
				}
			}
			else
			{
				m_Stack.Push(m_Nodes[num].ChildA);
				m_Stack.Push(m_Nodes[num].ChildB);
			}
		}
		return result;
	}

	public bool RayCast(float3 from, float3 to, RaycastCallback callback = null)
	{
		float3 v = math.normalize(to - from);
		float num = 1f;
		float3 x = math.normalize(FindOrthogonal(v));
		float3 x2 = math.abs(x);
		AABB b = AABB.Empty;
		b.Include(from);
		b.Include(to);
		m_Stack.Clear();
		m_Stack.Push(m_Root);
		bool result = false;
		while (m_Stack.Count > 0)
		{
			int num2 = m_Stack.Pop();
			if (num2 == Null || !AABB.Intersects(in m_Nodes[num2].Bounds, in b))
			{
				continue;
			}
			float3 center = m_Nodes[num2].Bounds.Center;
			float3 halfExtents = m_Nodes[num2].Bounds.HalfExtents;
			if (math.abs(math.dot(x, from - center)) - math.dot(x2, halfExtents) > 0f)
			{
				continue;
			}
			if (m_Nodes[num2].IsLeaf)
			{
				AABB bounds = m_Nodes[num2].Bounds;
				bounds.Expand(0f - FatBoundsRadius);
				if (!(bounds.RayCast(from, to, num) < 0f))
				{
					result = true;
					float num3 = callback?.Invoke(from, to, m_Nodes[num2].UserData) ?? num;
					if (num3 >= 0f)
					{
						num = num3;
						float3 y = from + num * (to - from);
						b.Min = math.min(from, y);
						b.Max = math.max(from, y);
					}
				}
			}
			else
			{
				m_Stack.Push(m_Nodes[num2].ChildA);
				m_Stack.Push(m_Nodes[num2].ChildB);
			}
		}
		return result;
	}

	private void InsertLeaf(int leaf)
	{
		if (m_Root == Null)
		{
			m_Root = leaf;
			m_Nodes[m_Root].Parent = Null;
			return;
		}
		AABB b = m_Nodes[leaf].Bounds;
		int num = m_Root;
		while (!m_Nodes[num].IsLeaf)
		{
			int childA = m_Nodes[num].ChildA;
			int childB = m_Nodes[num].ChildB;
			float halfArea = m_Nodes[num].Bounds.HalfArea;
			float halfArea2 = AABB.Union(in m_Nodes[num].Bounds, in b).HalfArea;
			float num2 = 2f * halfArea2;
			float num3 = 2f * (halfArea2 - halfArea);
			float num4;
			if (m_Nodes[childA].IsLeaf)
			{
				num4 = AABB.Union(in b, in m_Nodes[childA].Bounds).HalfArea + num3;
			}
			else
			{
				AABB aABB = AABB.Union(in b, in m_Nodes[childA].Bounds);
				float halfArea3 = m_Nodes[childA].Bounds.HalfArea;
				num4 = aABB.HalfArea - halfArea3 + num3;
			}
			float num5;
			if (m_Nodes[childB].IsLeaf)
			{
				num5 = AABB.Union(in b, in m_Nodes[childB].Bounds).HalfArea + num3;
			}
			else
			{
				AABB aABB2 = AABB.Union(in b, in m_Nodes[childB].Bounds);
				float halfArea4 = m_Nodes[childB].Bounds.HalfArea;
				num5 = aABB2.HalfArea - halfArea4 + num3;
			}
			if (num2 < num4 && num2 < num5)
			{
				break;
			}
			num = ((num4 < num5) ? childA : childB);
		}
		int num6 = num;
		int parent = m_Nodes[num6].Parent;
		int num7 = AllocateNode();
		m_Nodes[num7].Parent = parent;
		m_Nodes[num7].Bounds = AABB.Union(in b, in m_Nodes[num6].Bounds);
		m_Nodes[num7].Height = m_Nodes[num6].Height + 1;
		if (parent != Null)
		{
			if (m_Nodes[parent].ChildA == num6)
			{
				m_Nodes[parent].ChildA = num7;
			}
			else
			{
				m_Nodes[parent].ChildB = num7;
			}
			m_Nodes[num7].ChildA = num6;
			m_Nodes[num7].ChildB = leaf;
			m_Nodes[num6].Parent = num7;
			m_Nodes[leaf].Parent = num7;
		}
		else
		{
			m_Nodes[num7].ChildA = num6;
			m_Nodes[num7].ChildB = leaf;
			m_Nodes[num6].Parent = num7;
			m_Nodes[leaf].Parent = num7;
			m_Root = num7;
		}
		for (num = m_Nodes[leaf].Parent; num != Null; num = m_Nodes[num].Parent)
		{
			num = Balance(num);
			int childA2 = m_Nodes[num].ChildA;
			int childB2 = m_Nodes[num].ChildB;
			m_Nodes[num].Height = 1 + math.max(m_Nodes[childA2].Height, m_Nodes[childB2].Height);
			m_Nodes[num].Bounds = AABB.Union(in m_Nodes[childA2].Bounds, in m_Nodes[childB2].Bounds);
		}
	}

	private void RemoveLeaf(int leaf)
	{
		if (leaf == m_Root)
		{
			m_Root = Null;
			return;
		}
		int parent = m_Nodes[leaf].Parent;
		int parent2 = m_Nodes[parent].Parent;
		int num = ((m_Nodes[parent].ChildA == leaf) ? m_Nodes[parent].ChildB : m_Nodes[parent].ChildA);
		if (parent2 != Null)
		{
			if (m_Nodes[parent2].ChildA == parent)
			{
				m_Nodes[parent2].ChildA = num;
			}
			else
			{
				m_Nodes[parent2].ChildB = num;
			}
			m_Nodes[num].Parent = parent2;
			FreeNode(parent);
			int num2;
			for (num2 = parent2; num2 != Null; num2 = m_Nodes[num2].Parent)
			{
				num2 = Balance(num2);
				int childA = m_Nodes[num2].ChildA;
				int childB = m_Nodes[num2].ChildB;
				m_Nodes[num2].Bounds = AABB.Union(in m_Nodes[childA].Bounds, in m_Nodes[childB].Bounds);
				m_Nodes[num2].Height = 1 + Mathf.Max(m_Nodes[childA].Height, m_Nodes[childB].Height);
			}
		}
		else
		{
			m_Root = num;
			m_Nodes[num].Parent = Null;
			FreeNode(parent);
		}
	}

	private int Balance(int a)
	{
		if (m_Nodes[a].IsLeaf || m_Nodes[a].Height < 2)
		{
			return a;
		}
		int childA = m_Nodes[a].ChildA;
		int childB = m_Nodes[a].ChildB;
		int num = m_Nodes[childB].Height - m_Nodes[childA].Height;
		if (num > 1)
		{
			int childA2 = m_Nodes[childB].ChildA;
			int childB2 = m_Nodes[childB].ChildB;
			m_Nodes[childB].ChildA = a;
			m_Nodes[childB].Parent = m_Nodes[a].Parent;
			m_Nodes[a].Parent = childB;
			if (m_Nodes[childB].Parent != Null)
			{
				if (m_Nodes[m_Nodes[childB].Parent].ChildA == a)
				{
					m_Nodes[m_Nodes[childB].Parent].ChildA = childB;
				}
				else
				{
					m_Nodes[m_Nodes[childB].Parent].ChildB = childB;
				}
			}
			else
			{
				m_Root = childB;
			}
			if (m_Nodes[childA2].Height > m_Nodes[childB2].Height)
			{
				m_Nodes[childB].ChildB = childA2;
				m_Nodes[a].ChildB = childB2;
				m_Nodes[childB2].Parent = a;
				m_Nodes[a].Bounds = AABB.Union(in m_Nodes[childA].Bounds, in m_Nodes[childB2].Bounds);
				m_Nodes[childB].Bounds = AABB.Union(in m_Nodes[a].Bounds, in m_Nodes[childA2].Bounds);
				m_Nodes[a].Height = 1 + math.max(m_Nodes[childA].Height, m_Nodes[childB2].Height);
				m_Nodes[childB].Height = 1 + math.max(m_Nodes[a].Height, m_Nodes[childA2].Height);
			}
			else
			{
				m_Nodes[childB].ChildB = childB2;
				m_Nodes[a].ChildB = childA2;
				m_Nodes[childA2].Parent = a;
				m_Nodes[a].Bounds = AABB.Union(in m_Nodes[childA].Bounds, in m_Nodes[childA2].Bounds);
				m_Nodes[childB].Bounds = AABB.Union(in m_Nodes[a].Bounds, in m_Nodes[childB2].Bounds);
				m_Nodes[a].Height = 1 + math.max(m_Nodes[childA].Height, m_Nodes[childA2].Height);
				m_Nodes[childB].Height = 1 + math.max(m_Nodes[a].Height, m_Nodes[childB2].Height);
			}
			return childB;
		}
		if (num < -1)
		{
			int childA3 = m_Nodes[childA].ChildA;
			int childB3 = m_Nodes[childA].ChildB;
			m_Nodes[childA].ChildA = a;
			m_Nodes[childA].Parent = m_Nodes[a].Parent;
			m_Nodes[a].Parent = childA;
			if (m_Nodes[childA].Parent != Null)
			{
				if (m_Nodes[m_Nodes[childA].Parent].ChildA == a)
				{
					m_Nodes[m_Nodes[childA].Parent].ChildA = childA;
				}
				else
				{
					m_Nodes[m_Nodes[childA].Parent].ChildB = childA;
				}
			}
			else
			{
				m_Root = childA;
			}
			if (m_Nodes[childA3].Height > m_Nodes[childB3].Height)
			{
				m_Nodes[childA].ChildB = childA3;
				m_Nodes[a].ChildA = childB3;
				m_Nodes[childB3].Parent = a;
				m_Nodes[a].Bounds = AABB.Union(in m_Nodes[childB].Bounds, in m_Nodes[childB3].Bounds);
				m_Nodes[childA].Bounds = AABB.Union(in m_Nodes[a].Bounds, in m_Nodes[childA3].Bounds);
				m_Nodes[a].Height = 1 + math.max(m_Nodes[childB].Height, m_Nodes[childB3].Height);
				m_Nodes[childA].Height = 1 + math.max(m_Nodes[a].Height, m_Nodes[childA3].Height);
			}
			else
			{
				m_Nodes[childA].ChildB = childB3;
				m_Nodes[a].ChildA = childA3;
				m_Nodes[childA3].Parent = a;
				m_Nodes[a].Bounds = AABB.Union(in m_Nodes[childB].Bounds, in m_Nodes[childA3].Bounds);
				m_Nodes[childA].Bounds = AABB.Union(in m_Nodes[a].Bounds, in m_Nodes[childB3].Bounds);
				m_Nodes[a].Height = 1 + math.max(m_Nodes[childB].Height, m_Nodes[childA3].Height);
				m_Nodes[childA].Height = 1 + math.max(m_Nodes[a].Height, m_Nodes[childB3].Height);
			}
			return childA;
		}
		return a;
	}

	public static float3 FindOrthogonal(float3 v)
	{
		if (v.x >= Sqrt3Inv)
		{
			return new float3(v.y, 0f - v.x, 0f);
		}
		return new float3(0f, v.z, 0f - v.y);
	}

	public void DrawGizmos(int isolateDepth = -1)
	{
	}
}
