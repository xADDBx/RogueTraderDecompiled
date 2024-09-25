using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid.Passes;

public class FluidDebugPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fluid Debug";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fluid Debug");

	private Material m_BlitMaterial;

	private int m_PassDebug;

	private FluidFeature m_Feature;

	private FluidArea m_Area;

	private RenderTargetHandle m_ColorAttachment;

	private FluidFeature.FluidDebugSettings m_Settings;

	public FluidDebugPass(RenderPassEvent evt, Material blitMaterial)
	{
		base.RenderPassEvent = evt;
		m_BlitMaterial = blitMaterial;
		m_PassDebug = m_BlitMaterial.FindPass("Debug");
	}

	public void Setup(FluidFeature feature, FluidArea area, RenderTargetHandle colorAttachment, FluidFeature.FluidDebugSettings debugSettings)
	{
		m_Feature = feature;
		m_Area = area;
		m_ColorAttachment = colorAttachment;
		m_Settings = debugSettings;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (m_Area != null)
			{
				Vector4 value = new Vector4(1f, 1f, 0f, 0f);
				FluidAreaGpuBuffer gpuBuffer = m_Area.GpuBuffer;
				commandBuffer.SetGlobalVector(BlitBuffer._BlitScaleBias, value);
				commandBuffer.SetGlobalFloat(BlitBuffer._BlitMipLevel, 0f);
				commandBuffer.SetGlobalVector(FluidConstantBuffer._DebugColorScaleOffset, new Vector4(1f, 0f));
				float num = m_Settings.TextureSize;
				commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier());
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.StencilBuffer);
				commandBuffer.SetViewport(new Rect(0f, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalVector(FluidConstantBuffer._DebugColorScaleOffset, m_Feature.DebugSettings.VelocityColorScaleOffset);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.VelocityBuffer.Rt0);
				commandBuffer.SetViewport(new Rect(num, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.VelocityBuffer.Rt1);
				commandBuffer.SetViewport(new Rect(num, num, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalVector(FluidConstantBuffer._DebugColorScaleOffset, m_Feature.DebugSettings.DivergenceColorScaleOffset);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.DivergenceBuffer.Rt0);
				commandBuffer.SetViewport(new Rect(num * 2f, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalVector(FluidConstantBuffer._DebugColorScaleOffset, m_Feature.DebugSettings.PressureColorScaleOffset);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.PressureBuffer.Rt0);
				commandBuffer.SetViewport(new Rect(num * 3f, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.PressureBuffer.Rt1);
				commandBuffer.SetViewport(new Rect(num * 3f, num, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.FluidFogColorBuffer.Rt0);
				commandBuffer.SetViewport(new Rect(num * 4f, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, gpuBuffer.FluidFogColorBuffer.Rt1);
				commandBuffer.SetViewport(new Rect(num * 4f, num, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, m_PassDebug, MeshTopology.Triangles, 3, 1);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
