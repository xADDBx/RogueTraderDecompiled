using System;
using UnityEngine;

namespace Kingmaker.Visual;

public class CameraDisabler : IDisposable
{
	private Camera m_Camera;

	private readonly CameraStackManager.CameraStackType m_Type;

	public static CameraDisabler Disable(CameraStackManager.CameraStackType type)
	{
		return new CameraDisabler(type);
	}

	private CameraDisabler(CameraStackManager.CameraStackType type)
	{
		m_Type = type;
		m_Camera = CameraStackManager.Instance.GetCamera(type);
		if (!(m_Camera == null))
		{
			CameraStackManager.Instance.RemoveCamera(m_Camera, m_Type);
			m_Camera.enabled = false;
		}
	}

	public void Dispose()
	{
		if (m_Camera != null)
		{
			m_Camera.enabled = true;
			CameraStackManager.Instance.AddCamera(m_Camera, m_Type);
			m_Camera = null;
		}
	}
}
