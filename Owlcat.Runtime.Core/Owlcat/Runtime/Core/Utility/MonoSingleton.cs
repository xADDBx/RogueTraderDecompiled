using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static T s_Instance;

	protected virtual bool DontDestroyOnLoadThis => true;

	protected virtual Transform Parent => null;

	public static T Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = (T)Object.FindObjectOfType(typeof(T));
				if (Object.FindObjectsOfType(typeof(T)).Length > 1)
				{
					Debug.LogError("[MonoSingleton] Something went really wrong  - there should never be more than 1 singleton of type " + typeof(T).ToString() + "! Reopening the scene might fix it.");
					return s_Instance;
				}
				if (s_Instance == null)
				{
					Debug.Log("[MonoSingleton] There is no object of type " + typeof(T).ToString() + ". Creating new.");
					s_Instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
					s_Instance.gameObject.hideFlags = HideFlags.DontSave;
					if (s_Instance.DontDestroyOnLoadThis)
					{
						if (Application.isPlaying)
						{
							Object.DontDestroyOnLoad(s_Instance);
						}
					}
					else if (s_Instance.Parent != null)
					{
						s_Instance.transform.parent = s_Instance.Parent;
					}
				}
			}
			return s_Instance;
		}
	}
}
