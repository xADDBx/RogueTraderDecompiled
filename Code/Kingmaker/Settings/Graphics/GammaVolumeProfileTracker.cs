using System;
using Kingmaker.Visual;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Settings.Graphics;

internal sealed class GammaVolumeProfileTracker
{
	private bool m_Tracking;

	private CameraStackManager m_CameraStackManager;

	private VolumeProfile m_Profile;

	public VolumeProfile Profile
	{
		get
		{
			return m_Profile;
		}
		private set
		{
			if (!(m_Profile == value))
			{
				m_Profile = value;
				this.ProfileChanged?.Invoke(m_Profile);
			}
		}
	}

	public event Action<VolumeProfile> ProfileChanged;

	public void Start()
	{
		if (!m_Tracking)
		{
			m_Tracking = true;
			m_CameraStackManager = CameraStackManager.Instance;
			m_Profile = GetGammaProfile();
			m_CameraStackManager.StackChanged += OnCameraStackChanged;
		}
	}

	public void Stop()
	{
		m_CameraStackManager.StackChanged -= OnCameraStackChanged;
		m_Tracking = false;
		m_CameraStackManager = null;
		m_Profile = null;
	}

	private void OnCameraStackChanged(object sender, EventArgs e)
	{
		Profile = GetGammaProfile();
	}

	private VolumeProfile GetGammaProfile()
	{
		Camera camera = m_CameraStackManager.GetCamera(CameraStackManager.CameraStackType.Ui);
		if (camera == null)
		{
			return null;
		}
		Volume componentInChildren = camera.GetComponentInChildren<Volume>();
		if (componentInChildren == null)
		{
			return null;
		}
		return componentInChildren.profile;
	}
}
