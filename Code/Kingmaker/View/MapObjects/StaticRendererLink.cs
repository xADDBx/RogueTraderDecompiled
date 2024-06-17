using System.Linq;
using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.View.MapObjects;

public class StaticRendererLink : MonoBehaviour
{
	public string SceneName;

	public string RootName;

	public string TransformPath;

	public bool AddToHideRenderers = true;

	public Transform Reparent;

	private bool m_Linked;

	public void OnAreaBeginUnloading()
	{
	}

	public Transform FindLinkedTransform()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded && sceneAt.name == SceneName)
			{
				GameObject gameObject = sceneAt.GetRootGameObjects().FirstOrDefault((GameObject o) => o.name == RootName);
				if (!gameObject)
				{
					break;
				}
				return gameObject.transform.Find(TransformPath);
			}
		}
		return null;
	}

	public void OnAreaDidLoad()
	{
		DoLink();
	}

	public void DoLink()
	{
		if (m_Linked)
		{
			return;
		}
		Transform transform = FindLinkedTransform();
		if ((bool)transform)
		{
			if (AddToHideRenderers)
			{
				GetComponent<MapObjectView>().AddHideRenderer(transform.GetComponent<Renderer>());
			}
			if ((bool)Reparent)
			{
				transform.SetParent(Reparent, worldPositionStays: true);
				GetComponent<Highlighter>()?.ReinitMaterials();
			}
			m_Linked = true;
		}
	}
}
