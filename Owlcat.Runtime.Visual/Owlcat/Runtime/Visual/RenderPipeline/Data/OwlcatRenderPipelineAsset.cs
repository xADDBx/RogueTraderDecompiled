using Owlcat.Runtime.Visual.RenderPipeline.Decals;
using Owlcat.Runtime.Visual.RenderPipeline.PostProcess;
using Owlcat.Runtime.Visual.RenderPipeline.Shadows;
using Owlcat.Runtime.Visual.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data;

public class OwlcatRenderPipelineAsset : RenderPipelineAsset
{
	private Shader m_DefaultShader;

	private ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];

	[SerializeField]
	internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];

	[SerializeField]
	internal int m_DefaultRendererIndex;

	[SerializeField]
	private bool m_UseSRPBatcher = true;

	[SerializeField]
	private bool m_SupportsDynamicBatching = true;

	[SerializeField]
	private bool m_SupportsHDR;

	[SerializeField]
	private bool m_SupportsDistortion;

	[SerializeField]
	[Range(0.1f, 2f)]
	private float m_RenderScale = 1f;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateMode;

	[SerializeField]
	private DecalSettings m_DecalSettings = new DecalSettings();

	[SerializeField]
	private ShadowSettings m_ShadowSettings = new ShadowSettings();

	[SerializeField]
	private PostProcessSettings m_PostProcessSettings = new PostProcessSettings();

	[SerializeField]
	private TerrainSettings m_TerrainSettings = new TerrainSettings();

	public ScriptableRendererData[] RendererDataList => m_RendererDataList;

	public static float MinRenderScale => 0.1f;

	public static float MaxRenderScale => 2f;

	public ScriptableRenderer ScriptableRenderer
	{
		get
		{
			if (m_RendererDataList?.Length > m_DefaultRendererIndex && m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				Debug.LogError("Default renderer is missing from the current Pipeline Asset.", this);
				return null;
			}
			if (ScriptableRendererData.IsInvalidated || m_Renderers[m_DefaultRendererIndex] == null)
			{
				DestroyRenderer(ref m_Renderers[m_DefaultRendererIndex]);
				m_Renderers[m_DefaultRendererIndex] = ScriptableRendererData.InternalCreateRenderer();
			}
			return m_Renderers[m_DefaultRendererIndex];
		}
	}

	internal ScriptableRendererData ScriptableRendererData
	{
		get
		{
			if (m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				CreatePipeline();
			}
			return m_RendererDataList[m_DefaultRendererIndex];
		}
	}

	public bool UseSRPBatcher
	{
		get
		{
			return m_UseSRPBatcher;
		}
		set
		{
			m_UseSRPBatcher = value;
		}
	}

	public bool SupportsDynamicBatching
	{
		get
		{
			return m_SupportsDynamicBatching;
		}
		set
		{
			m_SupportsDynamicBatching = value;
		}
	}

	public bool SupportsHDR
	{
		get
		{
			return m_SupportsHDR;
		}
		set
		{
			m_SupportsHDR = value;
		}
	}

	public bool SupportsDistortion
	{
		get
		{
			return m_SupportsDistortion;
		}
		set
		{
			m_SupportsDistortion = value;
		}
	}

	public float RenderScale
	{
		get
		{
			return m_RenderScale;
		}
		set
		{
			m_RenderScale = ValidateRenderScale(value);
		}
	}

	public DecalSettings DecalSettings => m_DecalSettings;

	public ShadowSettings ShadowSettings => m_ShadowSettings;

	public PostProcessSettings PostProcessSettings => m_PostProcessSettings;

	public TerrainSettings TerrainSettings => m_TerrainSettings;

	public VolumeFrameworkUpdateMode VolumeFrameworkUpdateMode => m_VolumeFrameworkUpdateMode;

	public override Shader defaultShader
	{
		get
		{
			if (m_DefaultShader == null)
			{
				m_DefaultShader = Shader.Find("Owlcat/Lit");
			}
			return m_DefaultShader;
		}
	}

	public override Material defaultMaterial => GetMaterial(DefaultMaterialType.Standard);

	public override Material defaultParticleMaterial => GetMaterial(DefaultMaterialType.Particle);

	public override Material defaultTerrainMaterial => GetMaterial(DefaultMaterialType.Terrain);

	public override Material defaultUIMaterial => GetMaterial(DefaultMaterialType.UI);

	public override Material defaultUIOverdrawMaterial => GetMaterial(DefaultMaterialType.UnityBuiltinDefault);

	public Material DefaultSkyboxMaterial => GetMaterial(DefaultMaterialType.Skybox);

	public override Shader terrainDetailLitShader => defaultShader;

	public override Shader terrainDetailGrassBillboardShader => defaultShader;

	public override Shader terrainDetailGrassShader => defaultShader;

	protected override UnityEngine.Rendering.RenderPipeline CreatePipeline()
	{
		if (m_RendererDataList == null)
		{
			m_RendererDataList = new ScriptableRendererData[1];
		}
		if (m_RendererDataList[m_DefaultRendererIndex] == null)
		{
			Debug.LogError("Default Renderer is missing, make sure there is a Renderer assigned as the default on the current Owlcat RP asset:" + OwlcatRenderPipeline.Asset.name, this);
			return null;
		}
		CreateRenderers();
		return new OwlcatRenderPipeline(this);
	}

	private void DestroyRenderers()
	{
		if (m_Renderers != null)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				DestroyRenderer(ref m_Renderers[i]);
			}
		}
	}

	private void DestroyRenderer(ref ScriptableRenderer renderer)
	{
		if (renderer != null)
		{
			renderer.Dispose();
			renderer = null;
		}
	}

	protected override void OnValidate()
	{
		DestroyRenderers();
		base.OnValidate();
	}

	protected override void OnDisable()
	{
		DestroyRenderers();
		base.OnDisable();
	}

	private void CreateRenderers()
	{
		DestroyRenderers();
		if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
		{
			m_Renderers = new ScriptableRenderer[m_RendererDataList.Length];
		}
		for (int i = 0; i < m_RendererDataList.Length; i++)
		{
			if (m_RendererDataList[i] != null)
			{
				m_Renderers[i] = m_RendererDataList[i].InternalCreateRenderer();
			}
		}
	}

	private Material GetMaterial(DefaultMaterialType materialType)
	{
		ScriptableRendererData scriptableRendererData = m_RendererDataList[0];
		if (materialType == DefaultMaterialType.UI && scriptableRendererData != null)
		{
			ClusteredRendererData clusteredRendererData = scriptableRendererData as ClusteredRendererData;
			if (clusteredRendererData != null)
			{
				return clusteredRendererData.DefaultUIMaterial;
			}
		}
		return null;
	}

	public ScriptableRenderer GetRenderer(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length || index < 0 || m_RendererDataList[index] == null)
		{
			Debug.LogWarning("Renderer at index " + index + " is missing, falling back to Default Renderer " + m_RendererDataList[m_DefaultRendererIndex].name, this);
			index = m_DefaultRendererIndex;
		}
		if (m_Renderers == null || m_Renderers.Length < m_RendererDataList.Length)
		{
			CreateRenderers();
		}
		if (m_RendererDataList[index].IsInvalidated || m_Renderers[index] == null)
		{
			DestroyRenderer(ref m_Renderers[index]);
			m_Renderers[index] = m_RendererDataList[index].InternalCreateRenderer();
		}
		return m_Renderers[index];
	}

	public bool IsDefaultMaterial(Material material)
	{
		if (material == null)
		{
			return false;
		}
		if (material == default2DMaterial)
		{
			return true;
		}
		if (material == defaultLineMaterial)
		{
			return true;
		}
		if (material == defaultMaterial)
		{
			return true;
		}
		if (material == defaultParticleMaterial)
		{
			return true;
		}
		if (material == defaultTerrainMaterial)
		{
			return true;
		}
		if (material == GetDefaultDecalMaterial())
		{
			return true;
		}
		if (material == GetDefaultFullScreenDecalMaterial())
		{
			return true;
		}
		if (material == defaultUIETC1SupportedMaterial)
		{
			return true;
		}
		if (material == defaultUIMaterial)
		{
			return true;
		}
		if (material == defaultUIOverdrawMaterial)
		{
			return true;
		}
		if (material == DefaultSkyboxMaterial)
		{
			return true;
		}
		return false;
	}

	public Material GetDefaultDecalMaterial()
	{
		return GetMaterial(DefaultMaterialType.Decal);
	}

	public Material GetDefaultFullScreenDecalMaterial()
	{
		return GetMaterial(DefaultMaterialType.DecalFullScreen);
	}

	private float ValidateRenderScale(float value)
	{
		return Mathf.Max(MinRenderScale, Mathf.Min(value, MaxRenderScale));
	}

	internal bool ValidateRendererData(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length)
		{
			return false;
		}
		return m_RendererDataList[index] != null;
	}
}
