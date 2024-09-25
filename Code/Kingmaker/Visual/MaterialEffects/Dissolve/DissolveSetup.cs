using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.Dissolve;

public class DissolveSetup : MonoBehaviour
{
	public DissolveSettings Settings = new DissolveSettings();

	private void OnEnable()
	{
		Settings.Reset();
	}
}
