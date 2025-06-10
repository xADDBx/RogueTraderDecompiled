using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker;

public class SkinnedMeshRenderersCombiner : MonoBehaviour
{
	public SkinnedMeshRenderer SmrPrefab;

	[Tooltip("Character objects with SMR component")]
	public List<SkinnedMeshRenderer> SMRList = new List<SkinnedMeshRenderer>();

	public List<SkinnedMeshRenderer> SMRListHidden = new List<SkinnedMeshRenderer>();

	private static List<Transform> bones = new List<Transform>();

	[HideInInspector]
	[SerializeField]
	private List<BoneWeight> boneWeights = new List<BoneWeight>();

	private static List<CombineInstance> combineInstances = new List<CombineInstance>();

	private static List<Texture2D> textures = new List<Texture2D>();

	private static List<SkinnedMeshRenderer> smRenderers = new List<SkinnedMeshRenderer>();

	[HideInInspector]
	[SerializeField]
	private List<Matrix4x4> bindPoses = new List<Matrix4x4>();

	[HideInInspector]
	[SerializeField]
	private Vector2[] uv;

	[HideInInspector]
	[SerializeField]
	private Vector3[] verticies;

	[HideInInspector]
	[SerializeField]
	private int[] triangles;

	[SerializeField]
	private List<Material> allMaterials = new List<Material>();

	[Tooltip("Нужно на мешах, что в трансформе меняют размер")]
	[SerializeField]
	private bool m_useMatrices = true;

	[SerializeField]
	public string _texturePath = "Assets/Art/Characters/Creatures/";

	[SerializeField]
	public bool _addTexturesToPrefab;

	public int atlasWidth = 1024;

	public int atlasHeight = 512;

	public Texture2D AtlasTex;

	public Texture2D AtlasTexMask;

	public Texture2D AtlasTexNormal;

	public Texture2D[] testArray;

	public Rect[] Rects;

	public Material AtlasMaterial;

	public List<SkinnedMeshRenderer> SMRListFixedUV = new List<SkinnedMeshRenderer>();

	public void CombineMeshesF(GameObject o, Material m, List<SkinnedMeshRenderer> renderersToCombine, bool isAtlas = false)
	{
		bones.Clear();
		boneWeights.Clear();
		combineInstances.Clear();
		textures.Clear();
		smRenderers.Clear();
		bindPoses.Clear();
		Transform transform = null;
		smRenderers = renderersToCombine;
		if (AtlasMaterial == null)
		{
			AtlasMaterial = new Material(Shader.Find("Owlcat/Lit"));
			AtlasMaterial.SetFloat("_Metallic", 1f);
			AtlasMaterial.name = "AtlasMaterial";
		}
		foreach (SkinnedMeshRenderer item3 in renderersToCombine)
		{
			if (!(item3 == null) && !(item3.sharedMesh == null))
			{
				for (int i = 0; i < item3.sharedMesh.subMeshCount; i++)
				{
					CombineInstance combineInstance = default(CombineInstance);
					combineInstance.mesh = item3.sharedMesh;
					combineInstance.subMeshIndex = i;
					combineInstance.transform = item3.transform.localToWorldMatrix;
					CombineInstance item = combineInstance;
					combineInstances.Add(item);
				}
			}
		}
		int num = 0;
		for (int j = 0; j < renderersToCombine.Count; j++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = smRenderers[j];
			transform = ((transform == null) ? skinnedMeshRenderer.rootBone : transform);
			BoneWeight[] array = skinnedMeshRenderer.sharedMesh.boneWeights;
			for (int k = 0; k < array.Length; k++)
			{
				BoneWeight item2 = array[k];
				if (item2.weight0 > 0f)
				{
					item2.boneIndex0 += num;
				}
				if (item2.weight1 > 0f)
				{
					item2.boneIndex1 += num;
				}
				if (item2.weight2 > 0f)
				{
					item2.boneIndex2 += num;
				}
				if (item2.weight3 > 0f)
				{
					item2.boneIndex3 += num;
				}
				boneWeights.Add(item2);
			}
			num += skinnedMeshRenderer.bones.Length;
			Transform[] array2 = skinnedMeshRenderer.bones;
			for (int l = 0; l < array2.Length; l++)
			{
				bones.Add(array2[l]);
				bindPoses.Add(skinnedMeshRenderer.sharedMesh.bindposes[l] * skinnedMeshRenderer.transform.worldToLocalMatrix);
			}
		}
		SkinnedMeshRenderer skinnedMeshRenderer2 = o.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer2.enabled = false;
		skinnedMeshRenderer2.rootBone = transform;
		skinnedMeshRenderer2.sharedMesh = new Mesh();
		skinnedMeshRenderer2.sharedMesh.CombineMeshes(combineInstances.ToArray(), mergeSubMeshes: true, m_useMatrices);
		skinnedMeshRenderer2.sharedMesh.name = o.name + "_CombineMesh";
		Debug.Log("r.sharedMesh.vertexCount2:" + skinnedMeshRenderer2.sharedMesh.vertexCount);
		Debug.Log("Mesh.boneWeights:" + boneWeights.ToArray().Length);
		_ = skinnedMeshRenderer2.sharedMesh.uv;
		if (allMaterials.Count > 1)
		{
			skinnedMeshRenderer2.sharedMaterials = allMaterials.ToArray();
		}
		if (allMaterials.Count == 1 && !isAtlas)
		{
			skinnedMeshRenderer2.sharedMaterials = allMaterials.ToArray();
		}
		else
		{
			skinnedMeshRenderer2.sharedMaterial = AtlasMaterial;
		}
		if (isAtlas)
		{
			skinnedMeshRenderer2.sharedMaterials = new Material[0];
			skinnedMeshRenderer2.sharedMaterial = AtlasMaterial;
		}
		skinnedMeshRenderer2.bones = bones.ToArray();
		skinnedMeshRenderer2.sharedMesh.boneWeights = boneWeights.ToArray();
		skinnedMeshRenderer2.sharedMesh.bindposes = bindPoses.ToArray();
		skinnedMeshRenderer2.sharedMesh.RecalculateBounds();
		skinnedMeshRenderer2.sharedMesh.UploadMeshData(markNoLongerReadable: true);
		SmrPrefab = skinnedMeshRenderer2;
	}

	public void CombineSMR()
	{
	}

	public void CombineSMR_onlyOneObj()
	{
	}

	public void CreateOneMaterialUVMesh()
	{
		CombineAtlas();
		FixUV();
	}

	public void CombineAtlas()
	{
	}

	private Texture2D AddTextureToPrefabViaMemory(Texture2D sourceTexture, string name, string prefabPath)
	{
		return null;
	}

	public void FixUV()
	{
		SMRListFixedUV.Clear();
		foreach (SkinnedMeshRenderer sMR in SMRList)
		{
			SkinnedMeshRenderer item = Object.Instantiate(sMR);
			SMRListFixedUV.Add(item);
		}
		foreach (SkinnedMeshRenderer item2 in SMRListFixedUV)
		{
			int num = allMaterials.IndexOf(item2.sharedMaterial);
			if (num < 0 || num >= Rects.Length)
			{
				continue;
			}
			List<Vector2> list = new List<Vector2>();
			item2.sharedMesh = Object.Instantiate(item2.sharedMesh);
			item2.sharedMesh.GetUVs(0, list);
			for (int i = 0; i < list.Count; i++)
			{
				float num2 = list[i].x;
				float num3 = list[i].y;
				if (num2 < 0f || num2 > 1f)
				{
					num2 -= Mathf.Floor(num2);
				}
				if (num3 < 0f || num3 > 1f)
				{
					num3 -= Mathf.Floor(num3);
				}
				list[i] = new Vector2(num2 * Rects[num].width + Rects[num].x, num3 * Rects[num].height + Rects[num].y);
			}
			item2.sharedMesh.SetUVs(0, list);
		}
	}

	private void Start()
	{
		if (SmrPrefab != null)
		{
			SMRList.Remove(SmrPrefab);
		}
		foreach (SkinnedMeshRenderer sMR in SMRList)
		{
			sMR.enabled = false;
			sMR.gameObject.SetActive(value: false);
			Object.DestroyImmediate(sMR);
		}
		foreach (SkinnedMeshRenderer item in SMRListHidden)
		{
			item.enabled = false;
			item.gameObject.SetActive(value: false);
			Object.DestroyImmediate(item);
		}
		SMRList.Clear();
		SMRListHidden.Clear();
		SmrPrefab.enabled = true;
		SmrPrefab.gameObject.SetActive(value: true);
	}

	public Mesh Rebuild()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = verticies;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.boneWeights = boneWeights.ToArray();
		mesh.bindposes = bindPoses.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.UploadMeshData(markNoLongerReadable: true);
		return mesh;
	}
}
