using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[StructLayout(LayoutKind.Explicit)]
[BurstCompile]
public struct Node
{
	public struct InnerNodeData
	{
		public float leftPlane;

		public float rightPlane;
	}

	public struct LeafNodeData
	{
		public uint offset;

		public uint size;
	}

	private const uint kTypeMask = 805306368u;

	private const uint kChildIndexMask = 1073741823u;

	private const uint kLeafValue = 805306368u;

	private const int kAxisShift = 30;

	[FieldOffset(0)]
	private uint index;

	[FieldOffset(32)]
	private float innerLeftPlane;

	[FieldOffset(64)]
	private float innerRightPlane;

	[FieldOffset(32)]
	private uint leafOffset;

	[FieldOffset(64)]
	private uint leafSize;

	public int InnerChildIndex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(index & 0x3FFFFFFF);
		}
	}

	public float InnerLeftPlane
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return innerLeftPlane;
		}
	}

	public float InnerRightPlane
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return innerRightPlane;
		}
	}

	public int LeafOffset
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)leafOffset;
		}
	}

	public int LeafSize
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)leafSize;
		}
	}

	public bool IsLeafNode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (index & 0x30000000) == 805306368;
		}
	}

	public bool IsInnerNode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (index & 0x30000000) != 805306368;
		}
	}

	public int InnerSplitAxis
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(index >> 30);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Node(uint leafOffset, uint leafSize)
	{
		this = default(Node);
		index = 805306368u;
		this.leafOffset = leafOffset;
		this.leafSize = leafSize;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Node(uint splitAxis, uint childNodeIndex, float innerLeftPlane, float innerRightPlane)
	{
		this = default(Node);
		index = childNodeIndex | (splitAxis << 30);
		this.innerLeftPlane = innerLeftPlane;
		this.innerRightPlane = innerRightPlane;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node MakeInner(uint splitAxis, uint childNodeIndex, float leftPlane, float rightPlane)
	{
		return new Node(splitAxis, childNodeIndex, leftPlane, rightPlane);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node MakeInner(int splitAxis, int childNodeIndex, float leftPlane, float rightPlane)
	{
		return new Node((uint)splitAxis, (uint)childNodeIndex, leftPlane, rightPlane);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node MakeLeaf(uint boundsOffset, uint boundsSize)
	{
		return new Node(boundsOffset, boundsSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Node MakeLeaf(int boundsOffset, int boundsSize)
	{
		return new Node((uint)boundsOffset, (uint)boundsSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void MakeLeaf(ref Node node, uint boundsOffset, uint boundsSize)
	{
		node.index = 805306368u;
		node.leafOffset = boundsOffset;
		node.leafSize = boundsSize;
	}
}
