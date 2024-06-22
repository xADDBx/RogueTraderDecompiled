using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundSplineMover : MonoBehaviour
{
	public SoundSpline spline;

	private Transform thisTransform;

	private Transform followObj;

	private void Start()
	{
		thisTransform = base.transform;
		Scene sceneByName = SceneManager.GetSceneByName("BaseMechanics");
		if (sceneByName.isLoaded)
		{
			GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				Camera componentInChildren = rootGameObjects[i].GetComponentInChildren<Camera>();
				if (componentInChildren != null)
				{
					followObj = componentInChildren.transform;
					break;
				}
			}
			if (followObj == null)
			{
				Debug.LogError("MainCamera не найдена в другой сцене!");
			}
		}
		else
		{
			Debug.LogError("Другая сцена не загружена!");
		}
	}

	private void Update()
	{
		if (followObj != null && spline != null)
		{
			Vector3 position = followObj.position;
			thisTransform.position = spline.WhereOnSpline(position);
		}
	}
}
