using Owlcat.Runtime.UI.VirtualListSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.UI.Utility;

public class WidgetFactoryStash : MonoBehaviour
{
	private static WidgetFactoryStash s_Instance;

	public static WidgetFactoryStash Instance => s_Instance;

	public static bool Exists
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.enabled;
			}
			return false;
		}
	}

	public static void ResetStash()
	{
		WidgetFactory.DestroyAll();
		VirtualListViewsFabric.DestroyAll();
		if (s_Instance == null)
		{
			s_Instance = Object.FindObjectOfType<WidgetFactoryStash>();
			if (s_Instance == null && SceneManager.GetActiveScene().isLoaded)
			{
				s_Instance = new GameObject("[WidgetFactoryStash]").AddComponent<WidgetFactoryStash>();
			}
			s_Instance.transform.position = new Vector3(10000f, 10000f, 10000f);
			Object.DontDestroyOnLoad(s_Instance);
		}
	}

	private void LateUpdate()
	{
		WidgetFactory.DeactivateDisposedWidgets();
	}

	private void OnDestroy()
	{
		WidgetFactory.DestroyAll(fromOnDestroy: true);
		VirtualListViewsFabric.DestroyAll();
	}
}
