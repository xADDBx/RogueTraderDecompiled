using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

[DisallowMultipleComponent]
public class CameraAntialiasingHelper : MonoBehaviour
{
	private WaaaghAdditionalCameraData m_CameraData;

	private void OnEnable()
	{
		m_CameraData = GetComponent<WaaaghAdditionalCameraData>();
		UpdateSettings();
	}

	private void Update()
	{
		UpdateSettings();
	}

	private void UpdateSettings()
	{
		if (m_CameraData != null && SettingsController.Instance.GraphicsSettingsController != null)
		{
			WaaaghAdditionalCameraData cameraData = m_CameraData;
			cameraData.Antialiasing = SettingsController.Instance.GraphicsSettingsController.AntialiasingMode switch
			{
				AntialiasingMode.Off => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.None, 
				AntialiasingMode.SMAA => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.SubpixelMorphologicalAntiAliasing, 
				AntialiasingMode.TAA => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.TemporalAntialiasing, 
				_ => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.None, 
			};
			cameraData = m_CameraData;
			cameraData.AntialiasingQuality = SettingsController.Instance.GraphicsSettingsController.AntialiasingQuality switch
			{
				QualityOption.Low => AntialiasingQuality.Low, 
				QualityOption.Medium => AntialiasingQuality.Medium, 
				QualityOption.High => AntialiasingQuality.High, 
				_ => AntialiasingQuality.Low, 
			};
		}
	}
}
