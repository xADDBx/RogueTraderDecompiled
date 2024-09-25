using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.CustomPostProcess;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class EffectOverride
{
	private Dictionary<ShaderPropertyParameter, ShaderPropertyDescriptor> m_ParameterPropertyMap;

	public bool OverrideState;

	public CustomPostProcessEffect Effect;

	public List<ShaderPropertyParameter> Parameters;

	internal void ResetToDefault()
	{
		OverrideState = false;
		if (m_ParameterPropertyMap == null)
		{
			InitParameterPropertyMap();
		}
		foreach (KeyValuePair<ShaderPropertyParameter, ShaderPropertyDescriptor> item in m_ParameterPropertyMap)
		{
			item.Key.Property.SetValue(item.Value);
		}
	}

	private void InitParameterPropertyMap()
	{
		m_ParameterPropertyMap = new Dictionary<ShaderPropertyParameter, ShaderPropertyDescriptor>();
		foreach (ShaderPropertyParameter parameter in Parameters)
		{
			ShaderPropertyDescriptor value = Effect.Passes[parameter.PassIndex].Properties.FirstOrDefault((ShaderPropertyDescriptor p) => p.Name == parameter.Property.Name && p.Type == parameter.Property.Type);
			m_ParameterPropertyMap.Add(parameter, value);
		}
	}
}
