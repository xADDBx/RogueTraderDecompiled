using Kingmaker.Visual.Decals;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.CityBuilder;

[RequireComponent(typeof(MeshRenderer))]
public class CityBuilderTerrain : MonoBehaviour
{
	private MeshRenderer m_Renderer;

	private Texture m_OriginalTexture;

	private Material m_BakerMaterial;

	private Material m_MaterialClone;

	private RenderTexture m_RenderTexture;

	private Vector3[] m_DecalPoints = new Vector3[4]
	{
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f)
	};

	private Vector4[] m_DecalUv = new Vector4[4];

	public LayerMask RaycastMask;

	public static CityBuilderTerrain Instance { get; private set; }

	private void Start()
	{
		m_Renderer = GetComponent<MeshRenderer>();
		if (m_Renderer == null)
		{
			return;
		}
		Material sharedMaterial = m_Renderer.sharedMaterial;
		if (!(sharedMaterial == null))
		{
			m_OriginalTexture = sharedMaterial.mainTexture;
			if (!(m_OriginalTexture == null))
			{
				m_BakerMaterial = new Material(Shader.Find("Hidden/CityBuilderTerrainBaker"));
				m_RenderTexture = new RenderTexture(m_OriginalTexture.width, m_OriginalTexture.height, 0, RenderTextureFormat.ARGB32);
				m_RenderTexture.autoGenerateMips = true;
				m_RenderTexture.Create();
				Graphics.Blit(m_OriginalTexture, m_RenderTexture);
				m_MaterialClone = m_Renderer.material;
				m_MaterialClone.mainTexture = m_RenderTexture;
				m_Renderer.material = m_MaterialClone;
				Instance = this;
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_RenderTexture)
		{
			m_RenderTexture.Release();
			Object.DestroyImmediate(m_RenderTexture);
		}
		if ((bool)m_BakerMaterial)
		{
			Object.DestroyImmediate(m_BakerMaterial);
		}
		Instance = null;
	}

	public void BakeDecal(FxDecal decal)
	{
		BlitDecal(decal, remove: false);
	}

	public void RemoveDecal(FxDecal decal)
	{
		BlitDecal(decal, remove: true);
	}

	private void BlitDecal(FxDecal decal, bool remove)
	{
		bool flag = true;
		for (int i = 0; i < m_DecalPoints.Length; i++)
		{
			Vector3 origin = decal.transform.TransformPoint(m_DecalPoints[i]);
			if (Physics.Raycast(new Ray(origin, decal.transform.TransformDirection(Vector3.down)), out var hitInfo, decal.transform.lossyScale.y, RaycastMask))
			{
				m_DecalUv[i] = hitInfo.textureCoord;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			m_BakerMaterial.SetVectorArray(ShaderProps._Uv, m_DecalUv);
			m_BakerMaterial.SetFloat(ShaderProps._RemoveDecal, remove ? 1 : 0);
			Graphics.Blit(remove ? m_OriginalTexture : decal.SharedMaterial.GetTexture("_BaseMap"), m_RenderTexture, m_BakerMaterial, 0);
		}
	}
}
