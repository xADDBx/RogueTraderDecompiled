using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Sound;

public class SoundSplineMover : MonoBehaviour
{
	public SoundSpline spline;

	private Transform m_ThisTransform;

	private Camera m_Camera;

	private float maxDistance = 25f;

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
					m_Camera = componentInChildren;
					break;
				}
			}
			if (m_Camera == null)
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
		if (m_Camera != null && spline != null)
		{
			Vector3 point = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).GetPoint(maxDistance);
			m_ThisTransform.position = spline.WhereOnSpline(point);
		}
	}
}
