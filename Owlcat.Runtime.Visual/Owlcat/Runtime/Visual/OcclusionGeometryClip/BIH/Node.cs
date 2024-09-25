using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[StructLayout(LayoutKind.Explicit)]
[BurstCompile]
public struct Node
{
	private const uint kTypeMask = 3221225472u;

	private const uint kChildIndexMask = 1073741823u;

	private const uint kLeafValue = 3221225472u;

	private const int kAxisShift = 30;

	[FieldOffset(0)]
	private uint index;

	[FieldOffset(4)]
	private float innerLeftPlane;

	[FieldOffset(8)]
	private float innerRightPlane;

	[FieldOffset(4)]
	private uint leafOffset;

	[FieldOffset(8)]
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
			return (index & 0xC0000000u) == 3221225472u;
		}
	}

	public bool IsInnerNode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (index & 0xC0000000u) != 3221225472u;
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
		index = 3221225472u;
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
		node.index = 3221225472u;
		node.leafOffset = boundsOffset;
		node.leafSize = boundsSize;
	}
}
