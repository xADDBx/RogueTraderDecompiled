using System.Collections.Generic;
using UnityEngine;

namespace RogueTrader.SharedTypes;

public class OwlcatModificationMaterialsInSceneFixer : MonoBehaviour
{
	private void Awake()
	{
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		HashSet<Material> hashSet = new HashSet<Material>();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] materials = renderer.materials;
				foreach (Material item in materials)
				{
					hashSet.Add(item);
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				foreach (Material item2 in sharedMaterials)
				{
					hashSet.Add(item2);
				}
			}
		}
		PatchMaterialShaders(hashSet);
	}

	private static void PatchMaterialShaders(IEnumerable<Material> materials)
	{
		foreach (Material material in materials)
		{
			Shader shader = material.shader;
			if (shader != null)
			{
				material.shader = Shader.Find(shader.name) ?? shader;
			}
		}
	}
}
