using System;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Custom Post Process")]
public class CustomPostProcessOverride : VolumeComponent, IPostProcessComponent
{
	public CustomPostPocessParameter Settings = new CustomPostPocessParameter(new CustomPostProcessSettings());

	public static bool ValidateEffectOverride(EffectOverride effectOveride)
	{
		return true;
	}

	public EffectOverride AddEffect(CustomPostProcessEffect effect)
	{
		return Settings.AddEffect(effect);
	}

	public void UpdateEffectParameters(CustomPostProcessEffect effect)
	{
		Settings.UpdateEffectParameters(effect);
	}

	public bool IsActive()
	{
		foreach (EffectOverride effectOverride in Settings.value.EffectOverrides)
		{
			if (effectOverride.OverrideState)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return false;
	}

	internal void ApplyPropertiesOverride(CustomPostProcessEffect effect, CustomPostProcessEffectPass effectPass, Material material)
	{
		foreach (EffectOverride effectOverride in Settings.value.EffectOverrides)
		{
			if (!(effectOverride.Effect == effect))
			{
				continue;
			}
			int num = effect.Passes.IndexOf(effectPass);
			{
				foreach (ShaderPropertyParameter parameter in effectOverride.Parameters)
				{
					if (parameter.PassIndex == num)
					{
						parameter.Property.UpdateMaterial(material);
					}
				}
				break;
			}
		}
	}

	internal bool IsEffectActive(CustomPostProcessEffect effect)
	{
		foreach (EffectOverride effectOverride in Settings.value.EffectOverrides)
		{
			if (!ValidateEffectOverride(effectOverride))
			{
				effectOverride.OverrideState = false;
			}
			if (effectOverride.Effect == effect && effectOverride.OverrideState)
			{
				return true;
			}
		}
		return false;
	}
}
