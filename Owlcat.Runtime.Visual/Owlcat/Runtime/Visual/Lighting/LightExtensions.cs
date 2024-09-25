using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

public static class LightExtensions
{
	public static OwlcatAdditionalLightData GetOwlcatAdditionalLightData(this Light light)
	{
		GameObject gameObject = light.gameObject;
		if (!gameObject.TryGetComponent<OwlcatAdditionalLightData>(out var component))
		{
			return gameObject.AddComponent<OwlcatAdditionalLightData>();
		}
		return component;
	}
}
