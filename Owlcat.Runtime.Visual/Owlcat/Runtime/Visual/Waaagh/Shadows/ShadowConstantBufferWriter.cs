using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal readonly struct ShadowConstantBufferWriter
{
	private unsafe readonly ShadowConstantBuffer* m_BufferPtr;

	public unsafe ShadowConstantBufferWriter(in NativeReference<ShadowConstantBuffer> bufferReference)
	{
		m_BufferPtr = bufferReference.GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(int index, in ShadowData shadowData)
	{
		m_BufferPtr->_ShadowEntryParameters[index * 4] = GetPackedShadowFlags(in shadowData.LightData);
		m_BufferPtr->_ShadowEntryParameters[index * 4 + 1] = shadowData.RenderData.GPUDepthBiasArray[0];
		m_BufferPtr->_ShadowEntryParameters[index * 4 + 2] = shadowData.RenderData.GPUNormalBiasArray[0];
		int num = index * 4;
		for (int i = 0; i < shadowData.LightData.FaceCount; i++)
		{
			int num2 = num + i;
			for (int j = 0; j < 4; j++)
			{
				m_BufferPtr->_ShadowFaceAtlasScaleOffsets[num2 * 4 + j] = shadowData.DynamicAtlasData.ScaleOffsets[i][j];
			}
			ref ShadowFaceData reference = ref shadowData.RenderData.FaceDataArray[i];
			for (int k = 0; k < 4; k++)
			{
				m_BufferPtr->_ShadowFaceSpheres[num2 * 4 + k] = reference.CullingSphere[k];
			}
			for (int l = 0; l < 3; l++)
			{
				m_BufferPtr->_ShadowFaceDirections[num2 * 4 + l] = reference.FaceDirection[l];
			}
			for (int m = 0; m < 4; m++)
			{
				for (int n = 0; n < 4; n++)
				{
					int num3 = m * 4 + n;
					m_BufferPtr->_ShadowFaceMatrices[num2 * 16 + num3] = reference.WorldToShadow[m][n];
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetPackedShadowFlags(in ShadowLightData shadowLightData)
	{
		ShadowFlags shadowFlags = ShadowFlags.None;
		if (shadowLightData.LightType == LightType.Point)
		{
			shadowFlags |= ShadowFlags.Point;
		}
		if (shadowLightData.Shadows == LightShadows.Soft)
		{
			shadowFlags |= ShadowFlags.SoftShadows;
		}
		return math.asfloat((uint)shadowFlags);
	}
}
