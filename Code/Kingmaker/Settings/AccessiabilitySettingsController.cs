using Kingmaker.Settings.Graphics;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine.Rendering;

namespace Kingmaker.Settings;

public class AccessiabilitySettingsController
{
	private readonly AccessiabilitySettings m_Settings;

	private readonly GammaVolumeProfileTracker m_GammaVolumeProfileTracker;

	public AccessiabilitySettingsController()
	{
		m_Settings = SettingsRoot.Accessiability;
		m_GammaVolumeProfileTracker = new GammaVolumeProfileTracker();
		m_GammaVolumeProfileTracker.Start();
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			ApplyAccessibilitySettings(m_GammaVolumeProfileTracker.Profile);
		}
		m_GammaVolumeProfileTracker.ProfileChanged += OnProfileChanged;
		m_Settings.Protanopia.OnTempValueChanged += OnProtanopiaTempValueChanged;
		m_Settings.Deuteranopia.OnTempValueChanged += OnDeuteranopiaTempValueChanged;
		m_Settings.Tritanopia.OnTempValueChanged += OnTritanopiaTempValueChanged;
	}

	private void OnProfileChanged(VolumeProfile profile)
	{
		if (profile != null)
		{
			ApplyAccessibilitySettings(profile);
		}
	}

	private void ApplyAccessibilitySettings(VolumeProfile profile)
	{
		SetProtanopia(profile, m_Settings.Protanopia.GetValue());
		SetDeuteranopia(profile, m_Settings.Deuteranopia.GetValue());
		SetTritanopia(profile, m_Settings.Tritanopia.GetValue());
	}

	private void OnProtanopiaTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetProtanopia(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private void OnDeuteranopiaTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetDeuteranopia(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private void OnTritanopiaTempValueChanged(float value)
	{
		if (m_GammaVolumeProfileTracker.Profile != null)
		{
			SetTritanopia(m_GammaVolumeProfileTracker.Profile, value);
		}
	}

	private static void SetProtanopia(VolumeProfile profile, float value)
	{
		if (profile.TryGet<Daltonization>(out var component))
		{
			component.ProtanopiaFactor.value = value;
		}
	}

	private void SetDeuteranopia(VolumeProfile profile, float value)
	{
		if (profile.TryGet<Daltonization>(out var component))
		{
			component.DeuteranopiaFactor.value = value;
		}
	}

	private void SetTritanopia(VolumeProfile profile, float value)
	{
		if (profile.TryGet<Daltonization>(out var component))
		{
			component.TritanopiaFactor.value = value;
		}
	}
}
