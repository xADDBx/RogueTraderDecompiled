using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Utility;

public class BakedSkinnedMesh : MonoBehaviour
{
	private class CombineMeshInformation
	{
		public readonly bool ReceiveShadows;

		public readonly ShadowCastingMode ShadowCastingMode;

		public readonly bool AllowOcclusionWhenDynamic;

		public readonly uint RenderingLayerMask;

		private readonly List<CombineInstance> m_CombineInstances = new List<CombineInstance>();

		public IReadOnlyList<CombineInstance> CombineInstances => m_CombineInstances;

		public CombineMeshInformation(bool receiveShadows, ShadowCastingMode shadowCastingMode, bool allowOcclusionWhenDynamic, uint renderingLayerMask)
		{
			ReceiveShadows = receiveShadows;
			ShadowCastingMode = shadowCastingMode;
			AllowOcclusionWhenDynamic = allowOcclusionWhenDynamic;
			RenderingLayerMask = renderingLayerMask;
		}

		public void AddCombineInstance(CombineInstance combineInstance)
		{
			m_CombineInstances.Add(combineInstance);
		}
	}

	private bool m_IsBacked;

	private void OnEnable()
	{
		if (m_IsBacked)
		{
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		Dictionary<Material, CombineMeshInformation> dictionary = new Dictionary<Material, CombineMeshInformation>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (!dictionary.ContainsKey(skinnedMeshRenderer.sharedMaterial))
			{
				dictionary.Add(skinnedMeshRenderer.sharedMaterial, new CombineMeshInformation(skinnedMeshRenderer.receiveShadows, skinnedMeshRenderer.shadowCastingMode, skinnedMeshRenderer.allowOcclusionWhenDynamic, skinnedMeshRenderer.renderingLayerMask));
			}
			CombineMeshInformation combineMeshInformation = dictionary[skinnedMeshRenderer.sharedMaterial];
			Mesh mesh = new Mesh();
			skinnedMeshRenderer.BakeMesh(mesh);
			CombineInstance combineInstance = new CombineInstance
			{
				mesh = mesh
			};
			Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
			Transform obj = skinnedMeshRenderer.transform;
			Vector3 localScale = obj.parent.localScale;
			Vector3 localScale2 = obj.localScale;
			localScale.x = 1f / localScale.x / localScale2.x;
			localScale.y = 1f / localScale.y / localScale2.y;
			localScale.z = 1f / localScale.z / localScale2.z;
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.zero), localScale);
			combineInstance.transform = worldToLocalMatrix * skinnedMeshRenderer.localToWorldMatrix * matrix4x;
			combineMeshInformation.AddCombineInstance(combineInstance);
			skinnedMeshRenderer.enabled = false;
		}
		foreach (Material key in dictionary.Keys)
		{
			GameObject obj2 = new GameObject("backed - " + key.name);
			obj2.transform.parent = base.transform;
			obj2.transform.localPosition = Vector3.zero;
			obj2.transform.localScale = Vector3.one;
			obj2.transform.localRotation = Quaternion.Euler(Vector3.zero);
			MeshFilter meshFilter = obj2.AddComponent<MeshFilter>();
			meshFilter.mesh = new Mesh();
			meshFilter.mesh.CombineMeshes(dictionary[key].CombineInstances.ToArray(), mergeSubMeshes: true, useMatrices: true);
			MeshRenderer meshRenderer = obj2.AddComponent<MeshRenderer>();
			meshRenderer.material = key;
			meshRenderer.shadowCastingMode = dictionary[key].ShadowCastingMode;
			meshRenderer.receiveShadows = dictionary[key].ReceiveShadows;
			meshRenderer.allowOcclusionWhenDynamic = dictionary[key].AllowOcclusionWhenDynamic;
			meshRenderer.renderingLayerMask = dictionary[key].RenderingLayerMask;
		}
		m_IsBacked = true;
	}
}
