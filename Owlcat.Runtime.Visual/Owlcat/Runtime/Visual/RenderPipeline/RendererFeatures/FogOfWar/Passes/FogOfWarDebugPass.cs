using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.Passes;

public class FogOfWarDebugPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fog Of War Debug";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog Of War Debug");

	private FogOfWarArea m_Area;

	private Material m_BlitMaterial;

	public FogOfWarDebugPass(RenderPassEvent evt, Material blitMaterial)
	{
		base.RenderPassEvent = evt;
		m_BlitMaterial = blitMaterial;
	}

	public void Setup(FogOfWarArea area)
	{
		m_Area = area;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (m_Area != null)
			{
				Vector4 value = new Vector4(1f, 1f, 0f, 0f);
				commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, m_Area.FogOfWarMapRT);
				commandBuffer.SetGlobalVector(BlitBuffer._BlitScaleBias, value);
				commandBuffer.SetGlobalFloat(BlitBuffer._BlitMipLevel, 0f);
				float num = FogOfWarFeature.Instance.DebugSize;
				commandBuffer.SetViewport(new Rect(0f, 0f, num, num));
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
				if (m_Area.StaticMask != null)
				{
					commandBuffer.SetGlobalTexture(BlitBuffer._BlitTexture, m_Area.StaticMask);
					commandBuffer.SetViewport(new Rect(num, 0f, num, num));
					commandBuffer.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
				}
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
