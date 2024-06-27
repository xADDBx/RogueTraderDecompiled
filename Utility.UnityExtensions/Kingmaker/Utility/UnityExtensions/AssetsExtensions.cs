using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class AssetsExtensions
{
	public static bool IsPrefab(this GameObject gameObject)
	{
		if (gameObject != null)
		{
			return gameObject.scene.name == null;
		}
		return false;
	}

	public static IEnumerable<Object> GetPrefabInnerAssets(this GameObject prefab)
	{
		if (prefab == null || !prefab.IsPrefab())
		{
			yield break;
		}
		Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
		MeshFilter[] componentsInChildren = prefab.GetComponentsInChildren<MeshFilter>();
		MeshFilter[] array = componentsInChildren;
		foreach (MeshFilter meshFilter in array)
		{
			yield return meshFilter.sharedMesh;
		}
		Renderer[] array2 = renderers;
		foreach (Renderer renderer in array2)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				foreach (Texture2D item in material.GetAllTexturesFromMaterial())
				{
					yield return item;
				}
				yield return material;
			}
		}
	}

	public static IEnumerable<Texture2D> GetAllTexturesFromMaterial(this Material material)
	{
		if (material == null)
		{
			yield break;
		}
		string[] texturePropertyNames = material.GetTexturePropertyNames();
		string[] array = texturePropertyNames;
		foreach (string name in array)
		{
			if (material.GetTexture(name) is Texture2D texture2D)
			{
				yield return texture2D;
			}
		}
	}
}
