using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.ColorTint;

public class ColorTintAnimationSetup : MonoBehaviour
{
	public ColorTintAnimationSettings Settings;

	private void OnEnable()
	{
		Settings.Reset();
	}
}
