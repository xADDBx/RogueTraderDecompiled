using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.UnitLogic.FactLogic;

namespace Kingmaker.Controllers.FX;

public class CameraFXController : IController
{
	private Dictionary<RunPostEffect, float> m_EffectStartValueMap = new Dictionary<RunPostEffect, float>();

	public void RegisterRunningEffect(RunPostEffect runPostEffect, float startValue)
	{
		m_EffectStartValueMap[runPostEffect] = startValue;
	}

	public bool TryGetRunningEffectStartValue(VisualStateEffectType type, out float startValue)
	{
		bool result = false;
		startValue = 0f;
		foreach (KeyValuePair<RunPostEffect, float> item in m_EffectStartValueMap)
		{
			if (item.Key.EffectType == type)
			{
				startValue = item.Value;
				result = true;
				break;
			}
		}
		return result;
	}

	public void UnregisterRunningEffect(RunPostEffect runPostEffect)
	{
		m_EffectStartValueMap.Remove(runPostEffect);
	}
}
