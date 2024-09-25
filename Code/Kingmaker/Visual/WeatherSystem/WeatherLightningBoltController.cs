using Kingmaker.Sound.Base;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public class WeatherLightningBoltController : WeatherMinMaxRateSpawnController<WeatherLightningBoltSettings>
{
	public WeatherLightningBoltController(WeatherLightningBoltSettings settings, Transform root)
		: base(settings, root)
	{
	}

	protected override bool CanSpawn()
	{
		return m_Settings.LightningBoltPrefab != null;
	}

	protected override void Spawn(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if (Physics.Raycast(camera.ViewportPointToRay(new Vector3(PFStatefulRandom.Weather.value, PFStatefulRandom.Weather.value, 1f)), out var hitInfo, camera.farClipPlane, m_Settings.RaycastMask))
		{
			GameObject gameObject = FxHelper.SpawnFxOnPoint(m_Settings.LightningBoltPrefab, hitInfo.point);
			SoundEventsManager.PostEvent("WEATHER_Thunder_Distant_Single", gameObject);
		}
	}
}
