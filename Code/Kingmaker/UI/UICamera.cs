using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual;
using UnityEngine;

namespace Kingmaker.UI;

[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
	private static UICamera s_Instance;

	private Camera m_Camera;

	public static Camera Instance
	{
		get
		{
			if (!s_Instance)
			{
				return null;
			}
			return s_Instance.m_Camera;
		}
	}

	private static UICamera Prefab => BlueprintRoot.Instance.Prefabs.UICamera;

	[NotNull]
	public static Camera Claim()
	{
		if ((bool)Instance)
		{
			return Instance;
		}
		using (ProfileScope.New("Load UI Camera"))
		{
			if (Application.isPlaying && (bool)Prefab)
			{
				Object.Instantiate(Prefab);
			}
			return Instance;
		}
	}

	public void Awake()
	{
		m_Camera = GetComponent<Camera>();
		if (Application.isPlaying)
		{
			if (s_Instance != null)
			{
				Object.Destroy(this);
			}
			else
			{
				Object.DontDestroyOnLoad(this);
			}
		}
	}

	private void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
		}
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.AddCamera(m_Camera, CameraStackManager.CameraStackType.Ui);
		}
	}

	private void OnDisable()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.RemoveCamera(m_Camera, CameraStackManager.CameraStackType.Ui);
		}
	}
}
