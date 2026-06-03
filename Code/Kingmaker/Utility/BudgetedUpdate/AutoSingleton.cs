using UnityEngine;

namespace Kingmaker.Utility.BudgetedUpdate;

public static class AutoSingleton<T> where T : MonoBehaviour
{
	private static T s_Instance;

	public static T Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new GameObject().AddComponent<T>();
				Object.DontDestroyOnLoad(s_Instance.gameObject);
			}
			return s_Instance;
		}
	}

	public static void Destroy()
	{
		if ((bool)s_Instance)
		{
			Object.Destroy(s_Instance.gameObject);
			s_Instance = null;
		}
	}
}
