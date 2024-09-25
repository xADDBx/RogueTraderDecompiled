using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowCopyCacheConstantBufferWriter
{
	private unsafe readonly ShadowCopyCacheConstantBuffer* m_BufferPtr;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ShadowCopyCacheConstantBufferWriter(in NativeReference<ShadowCopyCacheConstantBuffer> bufferReference)
	{
		m_BufferPtr = bufferReference.GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(int entryIndex, in ShadowCacheCopyRequest copyRequest)
	{
		for (int i = 0; i < 4; i++)
		{
			m_BufferPtr->_ShadowDynamicAtlasScaleOffsets[entryIndex * 4 + i] = copyRequest.DynamicAtlasScaleOffset[i];
		}
		for (int j = 0; j < 4; j++)
		{
			m_BufferPtr->_ShadowCachedAtlasScaleOffsets[entryIndex * 4 + j] = copyRequest.StaticAtlasScaleOffset[j];
		}
	}
}
