using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker;

[ExecuteInEditMode]
public class ShadowProxyCombinerBox : MonoBehaviour
{
	public BoxCollider collider;

	public List<ShadowProxy> ShadowProxiesInside = new List<ShadowProxy>();

	public GameObject MergedShadowProxy;

	public void GetShadowProxiesInside()
	{
		ShadowProxiesInside.Clear();
		ShadowProxy[] array = Object.FindObjectsOfType<ShadowProxy>();
		foreach (ShadowProxy shadowProxy in array)
		{
			if (collider.bounds.Contains(shadowProxy.transform.position))
			{
				ShadowProxiesInside.Add(shadowProxy);
			}
		}
	}

	public void BakeShadowProxies()
	{
		GetShadowProxiesInside();
		if (ShadowProxiesInside.Count < 1)
		{
			return;
		}
		Material material = null;
		List<MeshFilter> list = new List<MeshFilter>();
		foreach (ShadowProxy item in ShadowProxiesInside)
		{
			MeshFilter component = item.GetComponent<MeshFilter>();
			MeshRenderer component2 = item.GetComponent<MeshRenderer>();
			if ((bool)component)
			{
				list.Add(component);
			}
			if (!material && (bool)component2)
			{
				material = component2.sharedMaterial;
			}
		}
		CombineInstance[] array = new CombineInstance[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i].mesh = list[i].sharedMesh;
			array[i].transform = list[i].transform.localToWorldMatrix;
			list[i].gameObject.SetActive(value: false);
		}
		MergedShadowProxy = new GameObject();
		MeshFilter meshFilter = MergedShadowProxy.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = MergedShadowProxy.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		if (material != null)
		{
			meshRenderer.material = material;
		}
		meshFilter.mesh = new Mesh();
		meshFilter.sharedMesh.CombineMeshes(array);
		meshFilter.name = "SHADOW_PROXIES_COMBINED";
		MergedShadowProxy.name = "SHADOW_PROXIES_COMBINED";
		MergedShadowProxy.transform.parent = base.transform;
	}
}
