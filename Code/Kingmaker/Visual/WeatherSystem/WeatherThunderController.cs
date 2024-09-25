using Kingmaker.Sound.Base;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public class WeatherThunderController : WeatherMinMaxRateSpawnController<WeatherThunderSettings>
{
	private ParticleSystem m_ParticleSystem;

	private ParticleSystem.MainModule m_Main;

	private Color m_StartMinColor;

	private Color m_StartMaxColor;

	private GradientColorKey[] m_StartMinColorKeys;

	private GradientColorKey[] m_StartMaxColorKeys;

	private GradientAlphaKey[] m_StartMinAlphaKeys;

	private GradientAlphaKey[] m_StartMaxAlphaKeys;

	private GradientAlphaKey[] m_CurrentMinAlphaKeys;

	private GradientAlphaKey[] m_CurrentMaxAlphaKeys;

	private Gradient m_CurrentMinGradient = new Gradient();

	private Gradient m_CurrentMaxGradient = new Gradient();

	private float m_LastWeatherIntensity = float.NegativeInfinity;

	public WeatherThunderController(WeatherThunderSettings settings, Transform root)
		: base(settings, root)
	{
	}

	protected override bool CanSpawn()
	{
		return m_Settings.ThunderPrefab != null;
	}

	protected override void Spawn(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if (m_ParticleSystem == null)
		{
			CreateParticleSystem(camera);
		}
		if (Mathf.Abs(m_LastWeatherIntensity - weatherIntensity) > 0.001f)
		{
			float num = m_Settings.AlphaOverIntensity.Evaluate(weatherIntensity);
			switch (m_Main.startColor.mode)
			{
			case ParticleSystemGradientMode.Color:
				m_StartMaxColor.a = num;
				m_Main.startColor = new ParticleSystem.MinMaxGradient(m_StartMaxColor);
				break;
			case ParticleSystemGradientMode.TwoColors:
				m_StartMinColor.a = num;
				m_StartMaxColor.a = num;
				m_Main.startColor = new ParticleSystem.MinMaxGradient(m_StartMinColor, m_StartMaxColor);
				break;
			case ParticleSystemGradientMode.Gradient:
			case ParticleSystemGradientMode.RandomColor:
			{
				for (int k = 0; k < m_CurrentMaxAlphaKeys.Length; k++)
				{
					m_CurrentMaxAlphaKeys[k].alpha = m_StartMaxAlphaKeys[k].alpha * num;
				}
				m_CurrentMaxGradient.SetKeys(m_StartMaxColorKeys, m_CurrentMaxAlphaKeys);
				m_Main.startColor = new ParticleSystem.MinMaxGradient(m_CurrentMaxGradient);
				break;
			}
			case ParticleSystemGradientMode.TwoGradients:
			{
				for (int i = 0; i < m_CurrentMinAlphaKeys.Length; i++)
				{
					m_CurrentMinAlphaKeys[i].alpha = m_StartMinAlphaKeys[i].alpha * num;
				}
				for (int j = 0; j < m_CurrentMaxAlphaKeys.Length; j++)
				{
					m_CurrentMaxAlphaKeys[j].alpha = m_StartMaxAlphaKeys[j].alpha * num;
				}
				m_CurrentMinGradient.SetKeys(m_StartMinColorKeys, m_CurrentMinAlphaKeys);
				m_CurrentMaxGradient.SetKeys(m_StartMaxColorKeys, m_CurrentMaxAlphaKeys);
				m_Main.startColor = new ParticleSystem.MinMaxGradient(m_CurrentMinGradient, m_CurrentMaxGradient);
				break;
			}
			}
		}
		m_LastWeatherIntensity = weatherIntensity;
		m_ParticleSystem.Emit(1);
		SoundEventsManager.PostEvent("WEATHER_Thunder_Distant_Single", m_ParticleSystem.gameObject);
	}

	private void CreateParticleSystem(Camera camera)
	{
		m_ParticleSystem = Object.Instantiate(m_Settings.ThunderPrefab, camera.transform, worldPositionStays: false);
		m_ParticleSystem.transform.rotation = Quaternion.identity;
		m_Main = m_ParticleSystem.main;
		switch (m_Main.startColor.mode)
		{
		case ParticleSystemGradientMode.Color:
		case ParticleSystemGradientMode.TwoColors:
			m_StartMinColor = m_Main.startColor.colorMin;
			m_StartMaxColor = m_Main.startColor.colorMax;
			break;
		case ParticleSystemGradientMode.Gradient:
		case ParticleSystemGradientMode.RandomColor:
			m_StartMaxColorKeys = m_Main.startColor.gradientMax.colorKeys;
			m_StartMaxAlphaKeys = m_Main.startColor.gradientMax.alphaKeys;
			m_CurrentMaxAlphaKeys = m_Main.startColor.gradientMax.alphaKeys;
			break;
		case ParticleSystemGradientMode.TwoGradients:
			m_StartMinColorKeys = m_Main.startColor.gradientMin.colorKeys;
			m_StartMaxColorKeys = m_Main.startColor.gradientMax.colorKeys;
			m_StartMinAlphaKeys = m_Main.startColor.gradientMin.alphaKeys;
			m_StartMaxAlphaKeys = m_Main.startColor.gradientMax.alphaKeys;
			m_CurrentMinAlphaKeys = m_Main.startColor.gradientMin.alphaKeys;
			m_CurrentMaxAlphaKeys = m_Main.startColor.gradientMax.alphaKeys;
			break;
		}
	}
}
