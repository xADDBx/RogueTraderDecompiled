using System.Collections.Generic;
using UnityEngine;

public class FX_MuzeonSpawnController : MonoBehaviour
{
	[Header("Shader Find and Replace")]
	public Shader findShader;

	public string findTag = "NecronDissolve";

	public Shader replaceShader;

	[Header("Dissolve Control")]
	public string dissolveMove = "_DissolveMove";

	[Range(-20f, 20f)]
	public float dissolveMoveValue = -20f;

	private GameObject groupMain;

	private List<Renderer> allRenderers = new List<Renderer>();

	private void Start()
	{
		groupMain = new GameObject("GroupMain");
		Renderer[] array = Object.FindObjectsOfType<Renderer>();
		List<GameObject> list = new List<GameObject>();
		Renderer[] array2 = array;
		foreach (Renderer renderer in array2)
		{
			if (!renderer.gameObject.isStatic || renderer.gameObject.tag != findTag)
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (material != null && material.shader == findShader)
				{
					list.Add(renderer.gameObject);
					break;
				}
			}
		}
		foreach (GameObject item in list)
		{
			item.transform.SetParent(groupMain.transform);
			Renderer component = item.GetComponent<Renderer>();
			if (component != null && replaceShader != null)
			{
				Material[] sharedMaterials = component.materials;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					sharedMaterials[i].shader = replaceShader;
				}
			}
		}
		allRenderers.Clear();
		allRenderers.AddRange(groupMain.GetComponentsInChildren<Renderer>());
	}

	private void Update()
	{
		if (allRenderers == null || allRenderers.Count == 0)
		{
			return;
		}
		foreach (Renderer allRenderer in allRenderers)
		{
			Material[] materials = allRenderer.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty(dissolveMove))
				{
					material.SetFloat(dissolveMove, dissolveMoveValue);
				}
			}
		}
	}
}
