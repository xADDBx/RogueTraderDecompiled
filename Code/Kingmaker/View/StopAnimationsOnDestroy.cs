using System.Collections.Generic;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using UnityEngine;

namespace Kingmaker.View;

public class StopAnimationsOnDestroy : MonoBehaviour
{
	private List<ColorTintAnimationSettings> m_ColorTintAnimationSettingses;

	private List<RimLightingAnimationSettings> m_RimLightingAnimationSettings;

	private List<DissolveSettings> m_DissolveSettingses;

	private List<AdditionalAlbedoSettings> m_AdditionalAlbedoSettingses;

	private List<MaterialParametersOverrideSettings> m_MaterialParametersOverrideSettings;

	private List<int> m_CustomPropertyAnimationTokens;

	private List<int> m_LayeredMaterialAnimationTokens;

	public StandardMaterialController MaterialAnimator { get; set; }

	private void OnDisable()
	{
		try
		{
			if (!MaterialAnimator)
			{
				return;
			}
			if (m_RimLightingAnimationSettings != null)
			{
				m_RimLightingAnimationSettings.ForEach(delegate(RimLightingAnimationSettings i)
				{
					MaterialAnimator.RimController.Animations.Remove(i);
				});
			}
			if (m_DissolveSettingses != null)
			{
				m_DissolveSettingses.ForEach(delegate(DissolveSettings i)
				{
					MaterialAnimator.DissolveController.Animations.Remove(i);
				});
			}
			if (m_ColorTintAnimationSettingses != null)
			{
				m_ColorTintAnimationSettingses.ForEach(delegate(ColorTintAnimationSettings i)
				{
					MaterialAnimator.ColorTintController.Animations.Remove(i);
				});
			}
			if (m_AdditionalAlbedoSettingses != null)
			{
				m_AdditionalAlbedoSettingses.ForEach(delegate(AdditionalAlbedoSettings i)
				{
					MaterialAnimator.AdditionalAlbedoController.Animations.Remove(i);
				});
			}
			if (m_MaterialParametersOverrideSettings != null)
			{
				m_MaterialParametersOverrideSettings.ForEach(delegate(MaterialParametersOverrideSettings i)
				{
					MaterialAnimator.MaterialParametersOverrideController.Entries.Remove(i);
				});
			}
			if (m_CustomPropertyAnimationTokens != null)
			{
				foreach (int customPropertyAnimationToken in m_CustomPropertyAnimationTokens)
				{
					MaterialAnimator.CustomMaterialPropertyAnimationController.RemoveAnimation(customPropertyAnimationToken);
				}
			}
			if (m_LayeredMaterialAnimationTokens == null)
			{
				return;
			}
			foreach (int layeredMaterialAnimationToken in m_LayeredMaterialAnimationTokens)
			{
				MaterialAnimator.RemoveOverlayAnimation(layeredMaterialAnimationToken);
			}
		}
		finally
		{
			MaterialAnimator = null;
			m_ColorTintAnimationSettingses?.Clear();
			m_RimLightingAnimationSettings?.Clear();
			m_DissolveSettingses?.Clear();
			m_AdditionalAlbedoSettingses?.Clear();
			m_MaterialParametersOverrideSettings?.Clear();
			m_CustomPropertyAnimationTokens?.Clear();
			m_LayeredMaterialAnimationTokens?.Clear();
		}
	}

	public void AddRimLightAnimationSetting(RimLightingAnimationSettings setting)
	{
		m_RimLightingAnimationSettings = m_RimLightingAnimationSettings ?? new List<RimLightingAnimationSettings>();
		m_RimLightingAnimationSettings.Add(setting);
	}

	public void AddDissolveAnimationSettings(DissolveSettings settings)
	{
		m_DissolveSettingses = m_DissolveSettingses ?? new List<DissolveSettings>();
		m_DissolveSettingses.Add(settings);
	}

	public void AddColorTintAnimationSettings(ColorTintAnimationSettings settings)
	{
		m_ColorTintAnimationSettingses = m_ColorTintAnimationSettingses ?? new List<ColorTintAnimationSettings>();
		m_ColorTintAnimationSettingses.Add(settings);
	}

	public void AddPetrificationAnimationSettings(AdditionalAlbedoSettings settings)
	{
		m_AdditionalAlbedoSettingses = m_AdditionalAlbedoSettingses ?? new List<AdditionalAlbedoSettings>();
		m_AdditionalAlbedoSettingses.Add(settings);
	}

	public void AddMaterialParametersOverrideSettings(MaterialParametersOverrideSettings settings)
	{
		m_MaterialParametersOverrideSettings = m_MaterialParametersOverrideSettings ?? new List<MaterialParametersOverrideSettings>();
		m_MaterialParametersOverrideSettings.Add(settings);
	}

	public void AddLayeredMaterialAnimationToken(int animationToken)
	{
		if (m_LayeredMaterialAnimationTokens == null)
		{
			m_LayeredMaterialAnimationTokens = new List<int>();
		}
		m_LayeredMaterialAnimationTokens.Add(animationToken);
	}

	internal void AddCustomPropertyAnimation(int token)
	{
		if (m_CustomPropertyAnimationTokens == null)
		{
			m_CustomPropertyAnimationTokens = new List<int>();
		}
		m_CustomPropertyAnimationTokens.Add(token);
	}
}
