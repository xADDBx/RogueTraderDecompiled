using UnityEngine;

namespace Kingmaker.Utility;

public class ApplicationQuitter : MonoBehaviour
{
	private static ApplicationQuitter s_Instance;

	private bool m_QuitRequested;

	private bool m_QuitRequestProcessing;

	internal static void Ensure()
	{
		if (!(s_Instance != null))
		{
			GameObject obj = new GameObject("ApplicationQuitter");
			s_Instance = obj.AddComponent<ApplicationQuitter>();
			Object.DontDestroyOnLoad(obj);
		}
	}

	internal static void Request()
	{
		s_Instance.m_QuitRequested = true;
	}

	private void Update()
	{
		if (m_QuitRequested && !m_QuitRequestProcessing)
		{
			m_QuitRequestProcessing = true;
			SystemUtil.ApplicationQuitInternal();
		}
	}

	private void OnApplicationQuit()
	{
		s_Instance = null;
	}
}
