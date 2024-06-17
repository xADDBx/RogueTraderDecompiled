using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class PostProcessPasses : IDisposable
{
	private ColorGradingLutPass m_ColorGradingLutPass;

	private BeforeTransparentPostProcessPass m_BeforeTransparentPostProcessPass;

	private PostProcessPass m_PostProcessPass;

	private PostProcessPass m_FinalPostProcessPass;

	private PostProcessData m_RendererPostProcessData;

	private PostProcessData m_CurrentPostProcessData;

	private Material m_BlitMaterial;

	public bool IsCreated => m_CurrentPostProcessData != null;

	public ColorGradingLutPass ColorGradingLutPass => m_ColorGradingLutPass;

	public BeforeTransparentPostProcessPass BeforeTransparentPostProcessPass => m_BeforeTransparentPostProcessPass;

	public PostProcessPass PostProcessPass => m_PostProcessPass;

	public PostProcessPass FinalPostProcessPass => m_FinalPostProcessPass;

	public PostProcessPasses(PostProcessData rendererPostProcessData, Material blitMaterial)
	{
		m_RendererPostProcessData = rendererPostProcessData;
		m_BlitMaterial = blitMaterial;
		Recreate(rendererPostProcessData);
	}

	public void Recreate(PostProcessData data)
	{
		if ((bool)m_RendererPostProcessData)
		{
			data = m_RendererPostProcessData;
		}
		if (!(data == m_CurrentPostProcessData))
		{
			if (m_CurrentPostProcessData != null)
			{
				m_ColorGradingLutPass?.Cleanup();
				m_BeforeTransparentPostProcessPass?.Cleanup();
				m_PostProcessPass?.Cleanup();
				m_FinalPostProcessPass?.Cleanup();
				m_ColorGradingLutPass = null;
				m_BeforeTransparentPostProcessPass = null;
				m_PostProcessPass = null;
				m_FinalPostProcessPass = null;
				m_CurrentPostProcessData = null;
			}
			if (data != null)
			{
				m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrePasses, data);
				m_BeforeTransparentPostProcessPass = new BeforeTransparentPostProcessPass(RenderPassEvent.BeforeRenderingOpaques, data, m_BlitMaterial);
				m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data, isFinalPostProcessPass: false);
				m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRenderingPostProcessing, data, isFinalPostProcessPass: true);
				m_CurrentPostProcessData = data;
			}
		}
	}

	public void Dispose()
	{
		m_ColorGradingLutPass.Cleanup();
		m_PostProcessPass.Cleanup();
		m_FinalPostProcessPass.Cleanup();
	}
}
