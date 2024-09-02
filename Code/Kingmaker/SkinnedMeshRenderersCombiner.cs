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
		for (int i = 0; i < allMaterials.Count; i++)
		{
			List<CombineInstance> list = new List<CombineInstance>();
			for (int j = 0; j < renderersToCombine.Count; j++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = smRenderers[j];
				for (int k = 0; k < skinnedMeshRenderer.sharedMaterials.Length; k++)
				{
					if (allMaterials[i] == skinnedMeshRenderer.sharedMaterials[k])
					{
						CombineInstance item = default(CombineInstance);
						item.mesh = skinnedMeshRenderer.sharedMesh;
						item.subMeshIndex = k;
						item.transform = skinnedMeshRenderer.transform.localToWorldMatrix;
						list.Add(item);
					}
				}
			}
			SkinnedMeshRenderer skinnedMeshRenderer2 = o.AddComponent<SkinnedMeshRenderer>();
			skinnedMeshRenderer2.enabled = false;
			skinnedMeshRenderer2.sharedMesh = new Mesh();
			skinnedMeshRenderer2.sharedMesh.CombineMeshes(list.ToArray(), mergeSubMeshes: true, useMatrices: true);
			CombineInstance item2 = default(CombineInstance);
			item2.mesh = skinnedMeshRenderer2.sharedMesh;
			Debug.Log("Material ci.mesh.vertexCount:" + item2.mesh.vertexCount);
			item2.transform = skinnedMeshRenderer2.transform.localToWorldMatrix;
			combineInstances.Add(item2);
			Object.DestroyImmediate(skinnedMeshRenderer2);
		}
		int num = 0;
		for (int l = 0; l < renderersToCombine.Count; l++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer3 = smRenderers[l];
			transform = ((transform == null) ? skinnedMeshRenderer3.rootBone : transform);
			BoneWeight[] array = skinnedMeshRenderer3.sharedMesh.boneWeights;
			for (int n = 0; n < array.Length; n++)
			{
				BoneWeight item3 = array[n];
				item3.boneIndex0 += num;
				item3.boneIndex1 += num;
				item3.boneIndex2 += num;
				item3.boneIndex3 += num;
				boneWeights.Add(item3);
			}
			num += skinnedMeshRenderer3.bones.Length;
			Transform[] array2 = skinnedMeshRenderer3.bones;
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				bones.Add(array2[num2]);
				bindPoses.Add(skinnedMeshRenderer3.sharedMesh.bindposes[num2] * skinnedMeshRenderer3.transform.worldToLocalMatrix);
			}
		}
		SkinnedMeshRenderer skinnedMeshRenderer4 = o.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer4.enabled = false;
		skinnedMeshRenderer4.rootBone = transform;
		skinnedMeshRenderer4.sharedMesh = new Mesh();
		skinnedMeshRenderer4.sharedMesh.CombineMeshes(combineInstances.ToArray(), mergeSubMeshes: true, m_useMatrices);
		skinnedMeshRenderer4.sharedMesh.name = o.name + "_CombineMesh";
		Debug.Log("r.sharedMesh.vertexCount2:" + skinnedMeshRenderer4.sharedMesh.vertexCount);
		Debug.Log("Mesh.boneWeights:" + boneWeights.ToArray().Length);
		_ = skinnedMeshRenderer4.sharedMesh.uv;
		if (allMaterials.Count > 1)
		{
			skinnedMeshRenderer4.sharedMaterials = allMaterials.ToArray();
		}
		if (allMaterials.Count == 1 && AtlasMaterial == null)
		{
			skinnedMeshRenderer4.sharedMaterials = allMaterials.ToArray();
		}
		else
		{
			skinnedMeshRenderer4.sharedMaterial = AtlasMaterial;
		}
		if (isAtlas)
		{
			skinnedMeshRenderer4.sharedMaterials = new Material[0];
			skinnedMeshRenderer4.sharedMaterial = AtlasMaterial;
		}
		skinnedMeshRenderer4.bones = bones.ToArray();
		skinnedMeshRenderer4.sharedMesh.boneWeights = boneWeights.ToArray();
		skinnedMeshRenderer4.sharedMesh.bindposes = bindPoses.ToArray();
		skinnedMeshRenderer4.sharedMesh.RecalculateBounds();
		skinnedMeshRenderer4.sharedMesh.UploadMeshData(markNoLongerReadable: true);
		SmrPrefab = skinnedMeshRenderer4;
	}

	public void CombineSMR()
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
			List<Vector2> list = new List<Vector2>();
			item2.sharedMesh = Object.Instantiate(item2.sharedMesh);
			item2.sharedMesh.GetUVs(0, list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = new Vector2(list[i].x * Rects[num].width + Rects[num].x, list[i].y * Rects[num].height + Rects[num].y);
				item2.sharedMesh.SetUVs(0, list);
			}
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
