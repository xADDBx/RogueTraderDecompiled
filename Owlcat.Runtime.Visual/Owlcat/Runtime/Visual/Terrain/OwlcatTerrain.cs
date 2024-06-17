using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Terrain;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.Terrain))]
public class OwlcatTerrain : MonoBehaviour
{
	public enum TerrainSizes
	{
		Fine_6,
		Diminutive_12,
		Tiny_25,
		Small_50,
		Small_75,
		Medium_100,
		Medium_150,
		Large_200,
		Large_250,
		Large_300,
		Huge_350,
		Huge_400
	}

	public enum Tex2DArrayResolution
	{
		x128 = 0x80,
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400
	}

	[HideInInspector]
	public TerrainSizes TerrainSize = TerrainSizes.Small_50;

	private static Vector4[] s_LayerDataMasksScale = new Vector4[256];

	private static Vector4[] s_LayerDataUvMatrix = new Vector4[256];

	private static Vector4[] s_LayerDataParams = new Vector4[256];

	[SerializeField]
	private Vector4[] m_LayerDataMasksScale = new Vector4[0];

	[SerializeField]
	private Vector4[] m_LayerDataUvMatrix = new Vector4[0];

	[SerializeField]
	private Vector4[] m_LayerDataParams = new Vector4[0];

	[SerializeField]
	private Texture2DArray m_Diffuse;

	[SerializeField]
	private Texture2DArray m_Normal;

	[SerializeField]
	private Texture2DArray m_Masks;

	private UnityEngine.Terrain m_Terrain;

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	public Tex2DArrayResolution TexturesResolution = Tex2DArrayResolution.x256;

	[Header("Splat")]
	[SerializeField]
	private Texture2DArray m_SplatArray;

	private Material m_BaseMapGenMat;

	public MaterialPropertyBlock MaterialPropertyBlock
	{
		get
		{
			if (m_MaterialPropertyBlock == null)
			{
				m_MaterialPropertyBlock = new MaterialPropertyBlock();
			}
			return m_MaterialPropertyBlock;
		}
	}

	public Texture2DArray SplatArray => m_SplatArray;

	public Texture2DArray DiffuseArray => m_Diffuse;

	public Texture2DArray NormalArray => m_Normal;

	public Texture2DArray MasksArray => m_Masks;

	public Material BaseMapGenMat
	{
		get
		{
			if (m_BaseMapGenMat == null)
			{
				m_BaseMapGenMat = CoreUtils.CreateEngineMaterial("Hidden/Owlcat/TerrainBaseGen");
			}
			return m_BaseMapGenMat;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	public void Init()
	{
		m_Terrain = GetComponent<UnityEngine.Terrain>();
		Texture2D[] alphamapTextures = m_Terrain.terrainData.alphamapTextures;
		for (int i = 0; i < alphamapTextures.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(alphamapTextures[i]);
		}
		ApplyMaterialProperties();
		m_Terrain.SetSplatMaterialPropertyBlock(MaterialPropertyBlock);
	}

	public void ApplyMaterialProperties()
	{
		if (m_LayerDataMasksScale.Length != 0)
		{
			Array.Copy(m_LayerDataMasksScale, s_LayerDataMasksScale, m_LayerDataMasksScale.Length);
			MaterialPropertyBlock.SetVectorArray("_TerrainLayerMasksScale", s_LayerDataMasksScale);
		}
		if (m_LayerDataUvMatrix.Length != 0)
		{
			Array.Copy(m_LayerDataUvMatrix, s_LayerDataUvMatrix, m_LayerDataUvMatrix.Length);
			MaterialPropertyBlock.SetVectorArray("_TerrainLayerUvMatrix", s_LayerDataUvMatrix);
		}
		if (m_LayerDataParams.Length != 0)
		{
			Array.Copy(m_LayerDataParams, s_LayerDataParams, m_LayerDataParams.Length);
			MaterialPropertyBlock.SetVectorArray("_TerrainLayerParams", s_LayerDataParams);
		}
		if (m_Diffuse != null)
		{
			MaterialPropertyBlock.SetTexture("_DiffuseArray", m_Diffuse);
		}
		if (m_Normal != null)
		{
			MaterialPropertyBlock.SetTexture("_NormalArray", m_Normal);
		}
		if (m_Masks != null)
		{
			MaterialPropertyBlock.SetTexture("_MasksArray", m_Masks);
		}
		MaterialPropertyBlock.SetInt("_ControlTexturesCount", m_Terrain.terrainData.alphamapTextureCount);
		if (m_SplatArray != null)
		{
			MaterialPropertyBlock.SetTexture("_SplatArray", m_SplatArray);
		}
		MaterialPropertyBlock.SetFloat("_TerrainMaxHeight", 1f / m_Terrain.terrainData.size.x);
		m_Terrain.SetSplatMaterialPropertyBlock(MaterialPropertyBlock);
	}

	public void BakeIfNeeded()
	{
	}
}
