using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Sound;

public class SoundSplineMover : MonoBehaviour
{
	public SoundSpline spline;

	private Transform m_ThisTransform;

	private Transform m_FollowObj;

	private void Start()
	{
		m_ThisTransform = base.transform;
		Scene sceneByName = SceneManager.GetSceneByName("BaseMechanics");
		if (sceneByName.isLoaded)
		{
			GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				Camera componentInChildren = rootGameObjects[i].GetComponentInChildren<Camera>();
				if (componentInChildren != null)
				{
					m_FollowObj = componentInChildren.transform;
					break;
				}
			}
			if (m_FollowObj == null)
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
		if (m_FollowObj != null && spline != null)
		{
			Vector3 position = m_FollowObj.position;
			m_ThisTransform.position = spline.WhereOnSpline(position);
		}
	}
}
