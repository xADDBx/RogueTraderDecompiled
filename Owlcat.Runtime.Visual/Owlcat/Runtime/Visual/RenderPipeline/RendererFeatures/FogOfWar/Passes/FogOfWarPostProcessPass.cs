using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.Passes;

public class FogOfWarPostProcessPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fog Of War Screen Space Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog Of War Screen Space Pass");

	private Material m_Material;

	private RenderTargetHandle m_ColorAttachment;

	private int m_ShaderPass;

	public FogOfWarPostProcessPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
		m_ShaderPass = m_Material.FindPass("DRAW FOW SCREEN SPACE");
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
			commandBuffer.Blit(BuiltinRenderTextureType.None, m_ColorAttachment.Identifier(), m_Material, m_ShaderPass);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
