using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class VFXLocationWeatherData : MonoBehaviour
{
	[HideInInspector]
	public Texture2D Texture;

	[HideInInspector]
	public List<VFXBakedGroundAreaAsset> Textures = new List<VFXBakedGroundAreaAsset>();

	private List<Mesh> m_DebugMeshes;

	private void OnEnable()
	{
		VFXTotalLocationWeatherData.AddArea(this);
	}

	private void OnDisable()
	{
		VFXTotalLocationWeatherData.RemoveArea(this);
	}

	public void SetCurrentWeatherProfile(WeatherProfileExtended weatherProfile)
	{
		VFXBakedGroundAreaAsset vFXBakedGroundAreaAsset = Textures.FirstOrDefault((VFXBakedGroundAreaAsset asset) => asset.Texture != null && asset.Matches(weatherProfile.Name));
		if (vFXBakedGroundAreaAsset == null)
		{
			Debug.LogWarning("[Baked Ground Area] No texture for profile '" + weatherProfile.Name + "' or '" + weatherProfile.LegacyName + "'.");
		}
		else
		{
			Texture = vFXBakedGroundAreaAsset.Texture;
			VFXTotalLocationWeatherData.AddArea(this);
		}
	}
}
