using Owlcat.Runtime.Visual.RenderPipeline.Shadows;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class ShadowCasterPass : ScriptableRenderPass
{
	private const string kShadowProcessTag = "Draw Shadows";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Shadows");

	private ClusteredShadows m_ClusteredShadows;

	private RenderTexture m_Shadowmap;

	public ShadowCasterPass(RenderPassEvent evt)
	{
		base.RenderPassEvent = evt;
	}

	public void Setup(ClusteredShadows clusteredShadows, RenderTexture shadowmap)
	{
		m_ClusteredShadows = clusteredShadows;
		m_Shadowmap = shadowmap;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			ShadowingData shadowData = renderingData.ShadowData;
			commandBuffer.SetRenderTarget(m_Shadowmap);
			commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			BindShadowData(commandBuffer, ref renderingData);
			if (m_ClusteredShadows.Entries.Count > 0)
			{
				ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.CullResults, 0, BatchCullingProjectionType.Unknown);
				for (int i = 0; i < m_ClusteredShadows.Entries.Count; i++)
				{
					ShadowmapEntry shadowmapEntry = m_ClusteredShadows.Entries[i];
					commandBuffer.SetGlobalInt(ShadowBuffer._ShadowEntryIndex, i);
					commandBuffer.SetGlobalFloat(ShadowBuffer._OffsetFactor, (shadowmapEntry.LightType == LightType.Point) ? shadowmapEntry.DepthBias : 0f);
					commandBuffer.SetGlobalFloat(ShadowBuffer._OffsetUnits, 0f);
					int b = 1;
					switch (shadowmapEntry.LightType)
					{
					case LightType.Spot:
						CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.GEOMETRY_CLIP, state: false);
						commandBuffer.SetGlobalInt(ShadowBuffer._ShadowFaceCount, 1);
						b = 1;
						break;
					case LightType.Directional:
					{
						CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.GEOMETRY_CLIP, shadowData.DirectionalLightCascades.Count > 1);
						if (shadowData.DirectionalLightCascades.Count > 1)
						{
							commandBuffer.SetGlobalVectorArray(ShadowBuffer._Clips, m_ClusteredShadows.GetDirectionalLightClips(shadowData.DirectionalLightCascades.Count));
							commandBuffer.SetGlobalInt(ShadowBuffer._ShadowFaceCount, shadowData.DirectionalLightCascades.Count);
						}
						Vector4 value = -shadowmapEntry.LocalToWorldMatrix.GetColumn(2);
						value.w = 0f;
						commandBuffer.SetGlobalVector(ShadowBuffer._LightDirection, value);
						b = shadowData.DirectionalLightCascades.Count;
						break;
					}
					case LightType.Point:
					{
						CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.GEOMETRY_CLIP, state: true);
						commandBuffer.SetGlobalVectorArray(ShadowBuffer._Clips, m_ClusteredShadows.PointLightClips);
						commandBuffer.SetGlobalFloat(ShadowBuffer._PunctualNearClip, shadowmapEntry.ShadowNearPlane);
						Vector4 column = shadowmapEntry.LocalToWorldMatrix.GetColumn(3);
						column.w = 1f;
						commandBuffer.SetGlobalVector(ShadowBuffer._LightDirection, column);
						commandBuffer.SetGlobalInt(ShadowBuffer._ShadowFaceCount, 4);
						b = 4;
						break;
					}
					}
					commandBuffer.SetViewport(shadowmapEntry.Viewport);
					context.ExecuteCommandBuffer(commandBuffer);
					commandBuffer.Clear();
					settings.lightIndex = shadowmapEntry.LightIndex;
					settings.useRenderingLayerMaskTest = true;
					b = Mathf.Max(1, b);
					for (int j = 0; j < b; j++)
					{
						commandBuffer.SetGlobalInt(ShadowBuffer._FaceId, j);
						commandBuffer.SetGlobalFloat(ShadowBuffer._ZClip, (shadowmapEntry.LightType != LightType.Directional) ? 1 : 0);
						context.ExecuteCommandBuffer(commandBuffer);
						commandBuffer.Clear();
						settings.splitData = shadowmapEntry.Splits[j];
						context.DrawShadows(ref settings);
					}
				}
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void BindShadowData(CommandBuffer cmd, ref RenderingData renderingData)
	{
		switch (renderingData.ShadowData.ShadowQuality)
		{
		case ShadowQuality.Disable:
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_HARD, state: false);
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_SOFT, state: false);
			break;
		case ShadowQuality.HardOnly:
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_HARD, state: true);
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_SOFT, state: false);
			break;
		case ShadowQuality.All:
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_HARD, state: false);
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SHADOWS_SOFT, state: true);
			break;
		}
		cmd.SetGlobalTexture(CommonTextureId._ShadowmapRT, m_Shadowmap);
		cmd.SetGlobalVectorArray(ShadowBuffer._FaceVectors, TetrahedronUtils.FaceVectors);
		cmd.SetGlobalBuffer(ComputeBufferId._ShadowMatricesBuffer, m_ClusteredShadows.ShadowMatricesBuffer);
		cmd.SetGlobalBuffer(ComputeBufferId._ShadowDataBuffer, m_ClusteredShadows.ShadowDataBuffer);
		GetScaleAndBiasForLinearDistanceFade(renderingData.CameraData.ShadowDistance, out var scale, out var bias);
		cmd.SetGlobalVector(ShadowBuffer._ShadowFadeDistanceScaleAndBias, new Vector4(scale, bias, 0f, 0f));
	}

	private void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, out float scale, out float bias)
	{
		float num = 0.9f * fadeDistance;
		scale = 1f / (fadeDistance - num);
		bias = (0f - num) / (fadeDistance - num);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
	}
}
