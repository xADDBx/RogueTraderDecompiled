using System;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal class NativeShadowCasterPass : ScriptableRenderPass<NativeShadowCasterPass.PassData>
{
	public class PassData : PassDataBase
	{
		public TextureHandle CachedShadowMapAtlasTexture;

		public TextureHandle ShadowMapAtlasTexture;

		public NativeList<ShadowRenderRequest> RenderRequestsForCache;

		public NativeList<ShadowRenderRequest> RenderRequests;

		public NativeList<ShadowCacheCopyRequest> ShadowCacheCopyRequests;

		public ShadowQuality ShadowQuality;

		public float ShadowDistance;

		public int DirectionalCascadesCount;

		public ShadowManager ShadowManager;

		public bool CacheEnabled;

		public Material CopyShadowsMaterial;

		public float ReceiverNormalBias;

		public int ShadowAtlasSize;
	}

	private static class PropertyId
	{
		public static readonly int _OutputNormalizedViewport = Shader.PropertyToID("_OutputNormalizedViewport");

		public static readonly int _InputDepthTex = Shader.PropertyToID("_InputDepthTex");
	}

	private static readonly ProfilerMarker s_ShadowCacheAtlasRenderMarker = new ProfilerMarker("Shadow Cache Atlas (Draw)");

	private static readonly ProfilerMarker s_ShadowAtlasCopyMarker = new ProfilerMarker("Shadow Atlas (Copy From Cache)");

	private static readonly ProfilerMarker s_ShadowAtlasRenderMarker = new ProfilerMarker("Shadow Atlas (Draw)");

	private Material m_CopyShadowsMaterial;

	public override string Name => "NativeShadowCasterPass";

	public NativeShadowCasterPass(RenderPassEvent evt, Material copyShadowsMaterial)
		: base(evt)
	{
		m_CopyShadowsMaterial = copyShadowsMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, PassData data, ref RenderingData renderingData)
	{
		TextureHandle input = data.Resources.NativeShadowmap;
		data.ShadowMapAtlasTexture = builder.WriteTexture(in input);
		ShadowManager shadowManager = renderingData.ShadowData.ShadowManager;
		data.RenderRequestsForCache = shadowManager.RenderRequestsForCache;
		data.RenderRequests = shadowManager.RenderRequests;
		data.ShadowCacheCopyRequests = shadowManager.ShadowCacheCopyRequests;
		data.ShadowQuality = renderingData.ShadowData.ShadowQuality;
		data.ShadowDistance = renderingData.CameraData.MaxShadowDistance;
		data.DirectionalCascadesCount = renderingData.ShadowData.DirectionalLightCascades.Count;
		data.ShadowManager = renderingData.ShadowData.ShadowManager;
		data.CacheEnabled = renderingData.ShadowData.StaticShadowsCacheEnabled;
		data.ReceiverNormalBias = renderingData.ShadowData.ReceiverNormalBias;
		data.ShadowAtlasSize = (int)renderingData.ShadowData.AtlasSize;
		if (data.CacheEnabled)
		{
			input = data.Resources.NativeCachedShadowmap;
			data.CachedShadowMapAtlasTexture = builder.ReadWriteTexture(in input);
			data.CopyShadowsMaterial = m_CopyShadowsMaterial;
		}
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		BindShadowData(data, context);
		NativeArray<ShadowRenderRequest> nativeArray;
		if (data.CacheEnabled)
		{
			if (data.RenderRequests.Length > 0)
			{
				context.cmd.SetRenderTarget(data.CachedShadowMapAtlasTexture);
				nativeArray = data.RenderRequestsForCache.AsArray();
				Span<ShadowRenderRequest> span = nativeArray.AsSpan();
				for (int i = 0; i < span.Length; i++)
				{
					DrawShadows(context, in span[i]);
				}
			}
			if (data.ShadowCacheCopyRequests.Length > 0)
			{
				context.cmd.SetRenderTarget(data.ShadowMapAtlasTexture);
				context.cmd.SetGlobalTexture(PropertyId._InputDepthTex, data.CachedShadowMapAtlasTexture);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.CopyShadowsMaterial, 0, MeshTopology.Quads, 4, data.ShadowCacheCopyRequests.Length);
			}
		}
		if (data.RenderRequests.Length > 0)
		{
			context.cmd.SetRenderTarget(data.ShadowMapAtlasTexture);
			nativeArray = data.RenderRequests.AsArray();
			Span<ShadowRenderRequest> span = nativeArray.AsSpan();
			for (int i = 0; i < span.Length; i++)
			{
				DrawShadows(context, in span[i]);
			}
		}
	}

	private void DrawShadows(RenderGraphContext context, in ShadowRenderRequest request)
	{
		context.cmd.SetGlobalInt(ShaderPropertyId._ShadowEntryIndex, request.ConstantBufferIndex);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OffsetFactor, request.DepthBias);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OffsetUnits, request.DepthBias);
		switch (request.LightType)
		{
		case LightType.Spot:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: false);
			context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[0]));
			if (request.NeedClear)
			{
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			break;
		case LightType.Point:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: true);
			context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[0]));
			if (request.NeedClear)
			{
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			break;
		case LightType.Directional:
			CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.GEOMETRY_CLIP, state: false);
			break;
		}
		for (int i = 0; i < request.FaceCount; i++)
		{
			if (request.LightType == LightType.Directional)
			{
				context.cmd.SetViewport(MakeViewportRect(in request.ShadowMapViewports[i]));
				context.cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black, 1f);
			}
			context.cmd.SetGlobalInt(ShaderPropertyId._FaceId, i);
			context.cmd.SetGlobalFloat(ShaderPropertyId._ZClip, (request.LightType != LightType.Directional) ? 1 : 0);
			context.cmd.DrawRendererList(request.RendererListArray[i]);
		}
	}

	private void BindShadowData(PassData data, RenderGraphContext context)
	{
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_HARD, data.ShadowQuality == ShadowQuality.HardOnly);
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.SHADOWS_SOFT, data.ShadowQuality == ShadowQuality.All);
		context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, data.ShadowMapAtlasTexture);
		context.cmd.SetGlobalFloat(ShaderPropertyId._ShadowReceiverNormalBias, data.ReceiverNormalBias);
		context.cmd.SetGlobalFloat(ShaderPropertyId._ShadowAtlasSize, data.ShadowAtlasSize);
		context.cmd.SetGlobalFloat(ShaderPropertyId._DirectionalCascadesCount, data.DirectionalCascadesCount);
		GetScaleAndBiasForLinearDistanceFade(data.ShadowDistance, out var scale, out var bias);
		context.cmd.SetGlobalVector(ShaderPropertyId._ShadowFadeDistanceScaleAndBias, new Vector4(scale, bias, 0f, 0f));
		data.ShadowManager.PushShadowConstantBuffer(context.cmd);
	}

	private void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, out float scale, out float bias)
	{
		float num = 0.9f * fadeDistance;
		scale = 1f / (fadeDistance - num);
		bias = (0f - num) / (fadeDistance - num);
	}

	private static Rect MakeViewportRect(in float4 v)
	{
		return new Rect(v.x, v.y, v.z, v.w);
	}
}
