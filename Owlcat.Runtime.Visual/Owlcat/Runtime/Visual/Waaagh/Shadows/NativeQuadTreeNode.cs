using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct NativeQuadTreeNode
{
	public NativeQuadTreeNodeState State;

	public float4 Rect;

	public short ParentIndex;

	public short ChildrenStartIndex;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public short GetChildIndex(short localChildIndex)
	{
		if (ChildrenStartIndex > -1)
		{
			return (short)(ChildrenStartIndex + localChildIndex);
		}
		return -1;
	}
}
