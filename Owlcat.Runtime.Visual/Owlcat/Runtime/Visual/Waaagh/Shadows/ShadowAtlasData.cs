using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowAtlasData
{
	public int AllocationSlotCount;

	public FixedArray4<int> SlotIndices;

	public FixedArray4<float4> Viewports;

	public FixedArray4<float4> ScaleOffsets;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	internal bool HasAllocation()
	{
		return AllocationSlotCount > 0;
	}
}
