using Kingmaker.Visual;
using UnityEngine;

public class RimLightSettings : MonoBehaviour
{
	public Color RimColor = Color.white;

	public float RimIntensity;

	private RenderingManager m_RenderingManager;

	private void Start()
	{
		if ((bool)RenderingManager.Instance)
		{
			m_RenderingManager = RenderingManager.Instance;
			m_RenderingManager.RimLighting.RimGlobalColor = RimColor;
			m_RenderingManager.RimLighting.RimGlobalIntensity = RimIntensity;
		}
	}
}
