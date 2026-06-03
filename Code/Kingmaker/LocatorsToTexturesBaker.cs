using System.Collections.Generic;
using Kingmaker.GPUCrowd;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker;

public class LocatorsToTexturesBaker : MonoBehaviour
{
	public VisualEffect m_CrowdVfx;

	public List<GpuCrowdLocator> m_CrowdLocators = new List<GpuCrowdLocator>();

	[Space]
	public string m_CountPropertyName = "ObjectsCount";

	[Space]
	public string m_PositionsPropertyName = "PositionMap";

	public Texture2D m_PositionsTexture;

	[Space]
	public string m_RotationsPropertyName = "RotationMap";

	public Texture2D m_RotationsTexture;

	[Space]
	public string m_ScalesPropertyName = "ScaleMap";

	public Texture2D m_ScalesTexture;

	[Space]
	public string m_BoundsSizePropertyName = "Bounds Size";

	public string m_BoundsCenterPropertyName = "Bounds Center";

	private void OnValidate()
	{
		if (m_CrowdVfx == null)
		{
			m_CrowdVfx = GetComponent<VisualEffect>();
		}
	}
}
