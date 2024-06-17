using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class FullscreenDebugPass : ScriptableRenderPass<FullscreenDebugPassData>
{
	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	public override string Name => "FullscreenDebugPass";

	public FullscreenDebugPass(RenderPassEvent evt, WaaaghDebugData debugData, Material debugMaterial)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Material = debugMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, FullscreenDebugPassData data, ref RenderingData renderingData)
	{
		if (!(m_DebugData == null) && !(m_Material == null))
		{
			if (m_DebugData.LightingDebug.DebugClustersMode == DebugClustersMode.DeferredLightingComplexity)
			{
				data.CameraDepthRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
			}
			if (m_DebugData.StencilDebug.StencilDebugType != 0)
			{
				TextureHandle input = data.Resources.FinalTarget;
				data.CameraFinalTarget = builder.WriteTexture(in input);
				data.CameraDepthRT = builder.WriteTexture(in data.Resources.CameraDepthBuffer);
				TextureDesc textureDesc = new TextureDesc(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
				textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
				TextureDesc desc = textureDesc;
				data.TempTarget = builder.CreateTransientTexture(in desc);
			}
			data.DebugData = m_DebugData;
			data.Material = m_Material;
			builder.AllowPassCulling(value: true);
		}
	}

	protected override void Render(FullscreenDebugPassData data, RenderGraphContext context)
	{
		if (!(data.DebugData == null) && !(data.Material == null))
		{
			switch (data.DebugData.LightingDebug.DebugClustersMode)
			{
			case DebugClustersMode.Heatmap:
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("TILES HEATMAP"), MeshTopology.Triangles, 3);
				break;
			case DebugClustersMode.HeatmapShadowedLights:
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("CLUSTERS HEATMAP SHADOWS"), MeshTopology.Triangles, 3);
				break;
			case DebugClustersMode.DeferredLightingComplexity:
				context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.CameraDepthRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("DEFERRED LIGHTING COMPLEXITY"), MeshTopology.Triangles, 3);
				break;
			}
			switch (data.DebugData.StencilDebug.StencilDebugType)
			{
			case StencilDebugType.Flags:
				context.cmd.SetRenderTarget(data.TempTarget, data.CameraDepthRT);
				context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
				context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
				context.cmd.SetGlobalFloat("_Debug_StecilRef", (float)m_DebugData.StencilDebug.Flags);
				context.cmd.SetGlobalFloat("_Debug_StecilReadMask", (float)m_DebugData.StencilDebug.Flags);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("STENCIL DEBUG"), MeshTopology.Triangles, 3);
				context.cmd.SetRenderTarget(data.CameraFinalTarget);
				context.cmd.SetGlobalTexture("_Debug_BlitInput", data.TempTarget);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("DEBUG BLIT"), MeshTopology.Triangles, 3);
				break;
			case StencilDebugType.Ref:
				context.cmd.SetRenderTarget(data.TempTarget, data.CameraDepthRT);
				context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, default(Color));
				context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
				context.cmd.SetGlobalFloat("_Debug_StecilRef", m_DebugData.StencilDebug.Ref);
				context.cmd.SetGlobalFloat("_Debug_StecilReadMask", 255f);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("STENCIL DEBUG"), MeshTopology.Triangles, 3);
				context.cmd.SetRenderTarget(data.CameraFinalTarget);
				context.cmd.SetGlobalTexture("_Debug_BlitInput", data.TempTarget);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, data.Material.FindPass("DEBUG BLIT"), MeshTopology.Triangles, 3);
				break;
			case StencilDebugType.None:
				break;
			}
		}
	}
}
