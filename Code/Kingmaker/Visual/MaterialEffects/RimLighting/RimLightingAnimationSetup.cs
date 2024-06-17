using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.RimLighting;

[ExecuteInEditMode]
public class RimLightingAnimationSetup : MonoBehaviour
{
	public RimLightingAnimationSettings Settings;

	private void OnEnable()
	{
		Settings.Reset();
	}

	private void OnDisable()
	{
		Settings.IsFinished = true;
	}
}
