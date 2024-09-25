using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;

public class FogOfWarSetupPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fog Of War Setup";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog Of War Setup");

	private FogOfWarSettings m_Fow;

	private FogOfWarArea m_Area;

	private RenderTextureDescriptor m_ShadowmapDesc;

	private Texture2D m_DefaultFogOfWarMask;

	public FogOfWarSetupPass(RenderPassEvent evt)
	{
		base.RenderPassEvent = evt;
	}

	public void Setup(FogOfWarSettings fow, FogOfWarArea area)
	{
		m_Area = area;
		m_Fow = fow;
		if (!(m_Area == null) && m_Area.isActiveAndEnabled)
		{
			m_ShadowmapDesc = new RenderTextureDescriptor(m_Area.FogOfWarMapRT.rt.width, m_Area.FogOfWarMapRT.rt.height, RenderTextureFormat.ARGB32);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (m_Area == null || !m_Area.isActiveAndEnabled)
			{
				Shader.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
				if (m_DefaultFogOfWarMask == null)
				{
					m_DefaultFogOfWarMask = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
					m_DefaultFogOfWarMask.name = "DefaultFowMask";
					m_DefaultFogOfWarMask.SetPixel(0, 0, new Color(1f, 1f, 0f, 0f));
					m_DefaultFogOfWarMask.Apply();
				}
				Shader.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, m_DefaultFogOfWarMask);
			}
			else
			{
				commandBuffer.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, m_Area.FogOfWarMapRT);
				commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMask_ST, m_Area.CalculateMaskST());
				Color color = m_Fow.Color;
				commandBuffer.SetGlobalColor(FogOfWarConstantBuffer._FogOfWarColor, color);
				commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, (!m_Area.ApplyShaderManually) ? 1 : 0);
				commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMaskSize, new Vector2(m_ShadowmapDesc.width, m_ShadowmapDesc.height));
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
