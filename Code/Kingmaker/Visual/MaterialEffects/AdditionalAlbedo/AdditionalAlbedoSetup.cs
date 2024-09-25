using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;

public class AdditionalAlbedoSetup : MonoBehaviour
{
	public AdditionalAlbedoSettings Settings;

	private void OnEnable()
	{
		Settings.Reset();
	}
}
