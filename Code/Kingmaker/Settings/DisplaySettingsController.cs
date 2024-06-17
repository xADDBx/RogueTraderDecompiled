using Kingmaker.Settings.Graphics;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Settings;

public class DisplaySettingsController
{
	private readonly DisplaySettings m_Settings;

	private readonly GammaVolumeProfileTracker m_GammaVolumeProfileTracker;

	public DisplaySettingsController()
	{
		m_Settings = SettingsRoot.Display;
		m_GammaVolumeProfileTracker = new GammaVolumeProfileTracker();
		m_GammaVolumeProfileTracker.Start();
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			ApplyDisplaySettings(m_GammaVolumeProfileTracker.Profile);
		}
		m_GammaVolumeProfileTracker.ProfileChanged += OnGammaProfileChanged;
		m_Settings.GammaCorrection.OnTempValueChanged += OnGammaCorrectionTempValueChanged;
		m_Settings.Brightness.OnTempValueChanged += OnBrightnessTempValueChanged;
		m_Settings.Contrast.OnTempValueChanged += OnContrastTempValueChanged;
	}

	private void OnGammaProfileChanged(VolumeProfile profile)
	{
		if (profile != null)
		{
			ApplyDisplaySettings(profile);
		}
	}

	private void ApplyDisplaySettings(VolumeProfile profile)
	{
		SetGamma(profile, m_Settings.GammaCorrection.GetValue());
		SetBrightness(profile, m_Settings.Brightness.GetValue());
		SetContrast(profile, m_Settings.Contrast.GetValue());
	}

	private void OnGammaCorrectionTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetGamma(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private void OnBrightnessTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetBrightness(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private void OnContrastTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetContrast(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private static void SetGamma(VolumeProfile profile, float value)
	{
		if (profile.TryGet<LiftGammaGain>(out var component))
		{
			Vector4 value2 = component.gamma.value;
			value2.w = Mathf.Lerp(-0.8f, 0.8f, value);
			component.gamma.value = value2;
		}
	}

	private static void SetBrightness(VolumeProfile profile, float value)
	{
		if (profile.TryGet<Daltonization>(out var component))
		{
			component.BrightnessFactor.value = value + 0.5f;
		}
	}

	private void SetContrast(VolumeProfile profile, float value)
	{
		if (profile.TryGet<Daltonization>(out var component))
		{
			component.ContrastFactor.value = value + 0.5f;
		}
	}
}
