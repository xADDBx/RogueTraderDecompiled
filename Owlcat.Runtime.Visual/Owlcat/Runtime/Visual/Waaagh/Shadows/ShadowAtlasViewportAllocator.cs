using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowAtlasViewportAllocator
{
	private ShadowAtlasAllocator m_AtlasAllocator;

	private readonly float m_AtlasSizeReciprocal;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ShadowAtlasViewportAllocator(ShadowAtlasAllocator atlasAllocator)
	{
		m_AtlasAllocator = atlasAllocator;
		m_AtlasSizeReciprocal = math.rcp(atlasAllocator.Resolution);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deallocate(ref ShadowAtlasData data)
	{
		for (int i = 0; i < data.AllocationSlotCount; i++)
		{
			m_AtlasAllocator.Free(data.SlotIndices[i]);
		}
		data.AllocationSlotCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Allocate(in ShadowLightData lightData, ref ShadowAtlasData atlasData)
	{
		bool flag = Allocate(lightData.ViewportCount, lightData.Resolution, ref atlasData);
		if (flag)
		{
			for (int i = lightData.ViewportCount; i < lightData.FaceCount; i++)
			{
				atlasData.Viewports[i] = atlasData.Viewports[0];
				atlasData.ScaleOffsets[i] = atlasData.ScaleOffsets[0];
			}
		}
		return flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Allocate(int viewportCount, int resolution, ref ShadowAtlasData atlasData)
	{
		for (int i = 0; i < viewportCount; i++)
		{
			if (!m_AtlasAllocator.Allocate(resolution, out var rect, out var slotIndex))
			{
				for (int j = 0; j < i; j++)
				{
					m_AtlasAllocator.Free(atlasData.SlotIndices[j]);
				}
				return false;
			}
			atlasData.SlotIndices[i] = slotIndex;
			atlasData.Viewports[i] = rect;
			atlasData.ScaleOffsets[i] = rect.zwxy * m_AtlasSizeReciprocal;
		}
		atlasData.AllocationSlotCount = viewportCount;
		return true;
	}
}
