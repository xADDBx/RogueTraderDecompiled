using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class ShadowsDebugPass : ScriptableRenderPass<ShadowsDebugPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_ShadowAtlasScaleOffset = Shader.PropertyToID("_Debug_ShadowAtlasScaleOffset");

		public static readonly int _Debug_ShadowAtlasColor = Shader.PropertyToID("_Debug_ShadowAtlasColor");

		public static readonly int _Debug_ShadowAtlasTex = Shader.PropertyToID("_Debug_ShadowAtlasTex");

		public static readonly int _Debug_ShadowAtlasMip = Shader.PropertyToID("_Debug_ShadowAtlasMip");
	}

	private WaaaghDebugData m_DebugData;

	private Material m_ShadowsDebugMaterial;

	private NativeQuadTreeDebugger m_QuadTreeDebugger;

	public override string Name => "ShadowsDebugPass";

	public ShadowsDebugPass(RenderPassEvent evt, WaaaghDebugData debugData, Material shadowsDebugMaterial)
		: base(evt)
	{
		m_DebugData = debugData;
		m_ShadowsDebugMaterial = shadowsDebugMaterial;
		m_QuadTreeDebugger = new NativeQuadTreeDebugger();
	}

	protected override void Setup(RenderGraphBuilder builder, ShadowsDebugPassData data, ref RenderingData renderingData)
	{
		data.ShadowData = renderingData.ShadowData;
		data.DebugData = m_DebugData;
		data.ScreenSize = new float2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
		data.ShadowsDebugMaterial = m_ShadowsDebugMaterial;
		TextureHandle input = data.Resources.FinalTarget;
		data.CameraFinalTarget = builder.WriteTexture(in input);
		data.ShadowsCacheEnabled = renderingData.ShadowData.StaticShadowsCacheEnabled;
		if (m_DebugData.ShadowsDebug.ViewAtlas == DebugShadowBufferType.None)
		{
			return;
		}
		switch (m_DebugData.ShadowsDebug.ViewAtlas)
		{
		case DebugShadowBufferType.ShadowmapAtlas:
			input = data.Resources.NativeShadowmap;
			data.ShadowBuffer = builder.ReadTexture(in input);
			break;
		case DebugShadowBufferType.CachedShadowmapAtlas:
			if (data.ShadowsCacheEnabled)
			{
				input = data.Resources.NativeCachedShadowmap;
				data.ShadowBuffer = builder.ReadTexture(in input);
			}
			break;
		case DebugShadowBufferType.None:
			break;
		}
	}

	protected override void Render(ShadowsDebugPassData data, RenderGraphContext context)
	{
		if (data.DebugData.ShadowsDebug.AtlasOccupancy != 0)
		{
			DrawNativeAtlasOccupancy(data, context);
		}
		if (data.DebugData.ShadowsDebug.ViewAtlas != 0 && (data.DebugData.ShadowsDebug.ViewAtlas != DebugShadowBufferType.CachedShadowmapAtlas || data.ShadowsCacheEnabled))
		{
			float2 screenSize = data.ScreenSize;
			float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.DebugData.ShadowsDebug.DebugScale) / screenSize;
			float2 float2 = 1f - @float;
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, float2.y);
			float4 float3 = data.DebugData.ShadowsDebug.DebugColorMultiplier;
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasColor, float3);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasScaleOffset, value);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_ShadowAtlasTex, data.ShadowBuffer);
			context.cmd.SetGlobalFloat(ShaderPropertyId._Debug_ShadowAtlasMip, 0f);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.ShadowsDebugMaterial, 0);
		}
	}

	private void DrawNativeAtlasOccupancy(ShadowsDebugPassData data, RenderGraphContext context)
	{
		ShadowManager shadowManager = data.ShadowData.ShadowManager;
		ShadowAtlas shadowAtlas = ((data.DebugData.ShadowsDebug.AtlasOccupancy == DebugShadowBufferType.ShadowmapAtlas) ? shadowManager.ShadowMapAtlas : shadowManager.CachedShadowMapAtlas);
		if (shadowAtlas != null)
		{
			m_QuadTreeDebugger.Refresh(shadowAtlas.Allocator.QuadTree, data.DebugData.ShadowsDebug.AtlasNodesPartiallyOccupied, data.DebugData.ShadowsDebug.AtlasNodesOccupied, data.DebugData.ShadowsDebug.AtlasNodesOccupiedInHierarchy);
			Texture2D allocationTexture = m_QuadTreeDebugger.AllocationTexture;
			int levels = shadowAtlas.Allocator.Levels;
			float num = data.ScreenSize.x / (float)levels;
			float2 screenSize = data.ScreenSize;
			float2 @float = num / screenSize;
			float2 float2 = (num - 5f) / screenSize;
			float2 float3 = -1f + @float;
			for (int i = 0; i < levels; i++)
			{
				Vector4 value = new Vector4(float2.x, float2.y, float3.x + (float)i * @float.x * 2f, float3.y);
				Vector4 value2 = new Vector4(1f, 1f, 1f, 1f);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasColor, value2);
				context.cmd.SetGlobalVector(ShaderPropertyId._Debug_ShadowAtlasScaleOffset, value);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_ShadowAtlasTex, allocationTexture);
				context.cmd.SetGlobalFloat(ShaderPropertyId._Debug_ShadowAtlasMip, i);
				context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.ShadowsDebugMaterial, 0);
			}
		}
	}
}
