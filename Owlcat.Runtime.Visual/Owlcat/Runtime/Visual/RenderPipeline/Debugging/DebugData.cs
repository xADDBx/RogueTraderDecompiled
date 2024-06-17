using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

public class DebugData : ScriptableObject
{
	[SerializeField]
	private Shader m_DebugShader;

	[SerializeField]
	private LightingDebug m_LightingDebug = new LightingDebug();

	[SerializeField]
	private RenderingDebug m_RenderingDebug = new RenderingDebug();

	[SerializeField]
	private TerrainDebug m_TerrainDebug = new TerrainDebug();

	[SerializeField]
	private OverdrawDebug m_OverdrawDebug = new OverdrawDebug();

	[SerializeField]
	private IndirectRenderingDebug m_IndirectRenderingDebug = new IndirectRenderingDebug();

	[SerializeField]
	private StencilDebug m_StencilDebug = new StencilDebug();

	private MipMapDebug m_MipMapDebug;

	public Shader DebugShader => m_DebugShader;

	public LightingDebug LightingDebug => m_LightingDebug;

	public RenderingDebug RenderingDebug => m_RenderingDebug;

	public TerrainDebug TerrainDebug => m_TerrainDebug;

	public OverdrawDebug OverdrawDebug => m_OverdrawDebug;

	public IndirectRenderingDebug IndirectRenderingDebug => m_IndirectRenderingDebug;

	public StencilDebug StencilDebug => m_StencilDebug;

	public MipMapDebug MipMapDebug
	{
		get
		{
			if (m_MipMapDebug == null)
			{
				m_MipMapDebug = new MipMapDebug();
			}
			return m_MipMapDebug;
		}
	}

	public bool IsDebugDisplayEnabled()
	{
		if (!m_RenderingDebug.DebugMipMap && m_LightingDebug.DebugLightingMode == DebugLightingMode.None && m_RenderingDebug.DebugBuffers == DebugBuffers.None && m_RenderingDebug.DebugMaterial == DebugMaterial.None && m_RenderingDebug.DebugVertexAttribute == DebugVertexAttribute.None && m_TerrainDebug.DebugTerrain == DebugTerrain.None)
		{
			return m_StencilDebug.StencilDebugType != StencilDebugType.None;
		}
		return true;
	}

	public bool IsAnyDebugEnabled()
	{
		if (!IsDebugDisplayEnabled() && m_LightingDebug.DebugClustersMode == DebugClustersMode.None)
		{
			return m_OverdrawDebug.OverdrawMode != OverdrawMode.None;
		}
		return true;
	}
}
