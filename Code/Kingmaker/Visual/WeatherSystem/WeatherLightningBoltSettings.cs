using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Lightning Bolt Effect")]
public class WeatherLightningBoltSettings : WeatherMinMaxRateSpawnSettings
{
	public LayerMask RaycastMask;

	public GameObject LightningBoltPrefab;

	public override IWeatherEntityController GetController(Transform root)
	{
		return new WeatherLightningBoltController(this, root);
	}
}
