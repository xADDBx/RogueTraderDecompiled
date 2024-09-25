using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data;

public class OwlcatRenderPipelineEditorResources : ScriptableObject
{
	[SerializeField]
	private Material m_DefaultMaterial;

	[SerializeField]
	private Material m_DefaultDecalMaterial;

	[SerializeField]
	private Material m_DefaultFullScreenDecalMaterial;

	[SerializeField]
	private Material m_DefaultTerrainMaterial;

	[SerializeField]
	private Material m_DefaultParticlesMaterial;

	[SerializeField]
	private Material m_DefaultSkyboxMaterial;

	[SerializeField]
	private Material m_DefaultUIMaterial;

	public Material DefaultMaterial => m_DefaultMaterial;

	public Material DefaultDecalMaterial => m_DefaultDecalMaterial;

	public Material DefaultFullScreenDecalMaterial => m_DefaultFullScreenDecalMaterial;

	public Material DefaultTerrainMaterial => m_DefaultTerrainMaterial;

	public Material DefaultParticlesMaterial => m_DefaultParticlesMaterial;

	public Material DefaultSkyboxMaterial => m_DefaultSkyboxMaterial;

	public Material DefaultUIMaterial => m_DefaultUIMaterial;
}
