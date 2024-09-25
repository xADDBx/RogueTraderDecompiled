using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class CustomPostPocessParameter : VolumeParameter<CustomPostProcessSettings>
{
	public CustomPostPocessParameter(CustomPostProcessSettings settings, bool overrideState = false)
		: base(settings, overrideState)
	{
	}

	public override void SetValue(VolumeParameter parameter)
	{
		for (int i = 0; i < m_Value.EffectOverrides.Count; i++)
		{
			EffectOverride effectOverride = m_Value.EffectOverrides[i];
			if (!CustomPostProcessOverride.ValidateEffectOverride(effectOverride))
			{
				if (effectOverride.Effect == null)
				{
					m_Value.EffectOverrides.RemoveAt(i);
					i--;
					continue;
				}
				UpdateEffectParameters(effectOverride.Effect);
			}
			effectOverride.ResetToDefault();
		}
	}

	public override void Interp(CustomPostProcessSettings from, CustomPostProcessSettings to, float t)
	{
		foreach (EffectOverride effectOverride2 in to.EffectOverrides)
		{
			if (CustomPostProcessOverride.ValidateEffectOverride(effectOverride2))
			{
				EffectOverride effectOverride = EnsureEffect(effectOverride2);
				if (!CustomPostProcessOverride.ValidateEffectOverride(effectOverride))
				{
					effectOverride.OverrideState = false;
				}
				if (effectOverride2.OverrideState)
				{
					effectOverride.OverrideState = effectOverride2.OverrideState;
					InterpEffect(effectOverride, effectOverride2, t);
				}
			}
		}
	}

	private EffectOverride EnsureEffect(EffectOverride effectOverride)
	{
		for (int num = m_Value.EffectOverrides.Count - 1; num >= 0; num--)
		{
			EffectOverride effectOverride2 = m_Value.EffectOverrides[num];
			if (effectOverride2.Effect == null)
			{
				m_Value.EffectOverrides.RemoveAt(num);
			}
			else if (effectOverride2.Effect == effectOverride.Effect)
			{
				return effectOverride2;
			}
		}
		return AddEffect(effectOverride.Effect);
	}

	private void InterpEffect(EffectOverride fromEffect, EffectOverride toEffect, float interpFactor)
	{
		int count = fromEffect.Parameters.Count;
		for (int i = 0; i < count; i++)
		{
			ShaderPropertyParameter shaderPropertyParameter = fromEffect.Parameters[i];
			ShaderPropertyParameter shaderPropertyParameter2 = toEffect.Parameters[i];
			if (shaderPropertyParameter2.OverrideState)
			{
				shaderPropertyParameter.OverrideState = shaderPropertyParameter2.OverrideState;
				shaderPropertyParameter.Property.Interp(shaderPropertyParameter.Property, shaderPropertyParameter2.Property, interpFactor);
			}
		}
	}

	public EffectOverride AddEffect(CustomPostProcessEffect effect)
	{
		EffectOverride effectOverride = new EffectOverride
		{
			Effect = effect,
			OverrideState = false,
			Parameters = new List<ShaderPropertyParameter>()
		};
		FindEffectParameters(effectOverride);
		m_Value.EffectOverrides.Add(effectOverride);
		return effectOverride;
	}

	private void FindEffectParameters(EffectOverride effectOverride)
	{
		effectOverride.Parameters.Clear();
		for (int i = 0; i < effectOverride.Effect.Passes.Count; i++)
		{
			CustomPostProcessEffectPass customPostProcessEffectPass = effectOverride.Effect.Passes[i];
			for (int j = 0; j < customPostProcessEffectPass.Properties.Count; j++)
			{
				ShaderPropertyParameter item = new ShaderPropertyParameter
				{
					PassIndex = i,
					OverrideState = false,
					Property = new ShaderPropertyDescriptor(customPostProcessEffectPass.Properties[j])
				};
				effectOverride.Parameters.Add(item);
			}
		}
	}

	internal void UpdateEffectParameters(CustomPostProcessEffect effect)
	{
		foreach (EffectOverride effectOverride in value.EffectOverrides)
		{
			if (effectOverride.Effect == effect)
			{
				effectOverride.Parameters.Clear();
				FindEffectParameters(effectOverride);
			}
		}
	}
}
