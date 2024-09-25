using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Utility;

public class LightmapBakerSettings : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _staticObjects;

	[SerializeField]
	private GameObject[] _specialShadowObjects;

	public void BeforeBakeTasks()
	{
		GameObject[] staticObjects = _staticObjects;
		for (int i = 0; i < staticObjects.Length; i++)
		{
			staticObjects[i].SetActive(value: true);
		}
		staticObjects = _specialShadowObjects;
		foreach (GameObject gameObject in staticObjects)
		{
			if (null != gameObject.GetComponent<MeshRenderer>())
			{
				gameObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
			}
		}
	}

	public void AfterBakeTasks()
	{
		GameObject[] staticObjects = _staticObjects;
		for (int i = 0; i < staticObjects.Length; i++)
		{
			staticObjects[i].SetActive(value: false);
		}
		staticObjects = _specialShadowObjects;
		foreach (GameObject gameObject in staticObjects)
		{
			if (null != gameObject.GetComponent<MeshRenderer>())
			{
				gameObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}
}
