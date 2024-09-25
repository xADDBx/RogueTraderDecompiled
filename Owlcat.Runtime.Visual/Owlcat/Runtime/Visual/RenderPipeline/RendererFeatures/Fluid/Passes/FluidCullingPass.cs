using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid.Passes;

public class FluidCullingPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fluid Culling";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fluid Culling");

	private Material m_Material;

	private int m_CullingPass;

	private FluidArea m_Area;

	public FluidCullingPass(RenderPassEvent evt, Material fluidMaterial)
	{
		base.RenderPassEvent = evt;
		m_Material = fluidMaterial;
		m_CullingPass = m_Material.FindPass("Culling");
	}

	public void Setup(FluidArea area)
	{
		m_Area = area;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetRenderTarget(m_Area.GpuBuffer.StencilBuffer);
			commandBuffer.SetViewProjectionMatrices(renderingData.CameraData.Camera.worldToCameraMatrix, renderingData.CameraData.Camera.projectionMatrix);
			Bounds worldBounds = m_Area.GetWorldBounds();
			commandBuffer.SetGlobalVector(FluidConstantBuffer._FluidWorldBoundsMin, worldBounds.min);
			commandBuffer.SetGlobalVector(FluidConstantBuffer._FluidWorldBoundsSize, worldBounds.size);
			commandBuffer.DrawProcedural(Matrix4x4.identity, m_Material, m_CullingPass, MeshTopology.Triangles, 3);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
