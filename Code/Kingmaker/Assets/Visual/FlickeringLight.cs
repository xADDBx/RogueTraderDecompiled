using System;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Assets.Visual;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
	public enum FlickerinLightStyles
	{
		CampFire,
		Fluorescent
	}

	public enum CampfireMethods
	{
		Intensity,
		Range,
		Both
	}

	public enum CampfireIntesityStyles
	{
		Sine,
		Random
	}

	public enum CampfireRangeStyles
	{
		Sine,
		Random
	}

	public float IterationInterval = 0.1f;

	public FlickerinLightStyles FlickeringLightStyle;

	public CampfireMethods CampfireMethod;

	public CampfireIntesityStyles CampfireIntesityStyle = CampfireIntesityStyles.Random;

	public CampfireRangeStyles CampfireRangeStyle = CampfireRangeStyles.Random;

	public float CampfireIntensityBaseValue = 0.5f;

	public float CampfireIntensityFlickerValue = 0.1f;

	public float CampfireRangeBaseValue = 10f;

	public float CampfireRangeFlickerValue = 2f;

	private float m_CampfireSineCycleIntensity;

	private float m_CampfireSineCycleRange;

	public float CampfireSineCycleIntensitySpeed = 5f;

	public float CampfireSineCycleRangeSpeed = 5f;

	public float FluorescentFlickerMin = 0.4f;

	public float FluorescentFlickerMax = 0.5f;

	public float FluorescentFlicerPercent = 0.95f;

	private Light m_Light;

	private float m_LastTime;

	private void Start()
	{
		m_Light = GetComponent<Light>();
	}

	private void Update()
	{
		if (!m_Light)
		{
			PFLog.Default.Error(this, "No light object");
		}
		else
		{
			if (m_LastTime + IterationInterval > Time.time)
			{
				return;
			}
			switch (FlickeringLightStyle)
			{
			case FlickerinLightStyles.CampFire:
				if (CampfireMethod == CampfireMethods.Intensity || CampfireMethod == CampfireMethods.Both)
				{
					if (CampfireIntesityStyle == CampfireIntesityStyles.Sine)
					{
						m_CampfireSineCycleIntensity += CampfireSineCycleIntensitySpeed;
						if (m_CampfireSineCycleIntensity > 360f)
						{
							m_CampfireSineCycleIntensity = 0f;
						}
						m_Light.intensity = CampfireIntensityBaseValue + (Mathf.Sin(m_CampfireSineCycleIntensity * (MathF.PI / 180f)) * (CampfireIntensityFlickerValue / 2f) + CampfireIntensityFlickerValue / 2f);
					}
					else
					{
						m_Light.intensity = CampfireIntensityBaseValue + PFStatefulRandom.Visual.Range(0f, CampfireIntensityFlickerValue);
					}
				}
				if (CampfireMethod != CampfireMethods.Range && CampfireMethod != CampfireMethods.Both)
				{
					break;
				}
				if (CampfireRangeStyle == CampfireRangeStyles.Sine)
				{
					m_CampfireSineCycleRange += CampfireSineCycleRangeSpeed;
					if (m_CampfireSineCycleRange > 360f)
					{
						m_CampfireSineCycleRange = 0f;
					}
					m_Light.range = CampfireRangeBaseValue + (Mathf.Sin(m_CampfireSineCycleRange * (MathF.PI / 180f)) * (m_CampfireSineCycleRange / 2f) + m_CampfireSineCycleRange / 2f);
				}
				else
				{
					m_Light.range = CampfireRangeBaseValue + PFStatefulRandom.Visual.Range(0f, CampfireRangeFlickerValue);
				}
				break;
			case FlickerinLightStyles.Fluorescent:
				m_Light.intensity = ((PFStatefulRandom.Visual.Range(0f, 1f) > FluorescentFlicerPercent) ? FluorescentFlickerMin : FluorescentFlickerMax);
				break;
			}
			m_LastTime = Time.time;
		}
	}
}
