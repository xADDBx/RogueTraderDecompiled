using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class FogPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Draw Fog";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Fog");

	private RenderTargetHandle m_ColorAttachment;

	private Material m_Material;

	public FogPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
	}

	public void Setup(RenderTargetHandle colorAttachment)
	{
		m_ColorAttachment = colorAttachment;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier());
			commandBuffer.DrawProcedural(Matrix4x4.identity, m_Material, 0, MeshTopology.Triangles, 3);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
