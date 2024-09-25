using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;

public class MaterialParametersOverrideSetup : MonoBehaviour
{
	public MaterialParametersOverrideSettings Settings;

	private void OnEnable()
	{
		Settings.Reset();
	}

	private void OnDisable()
	{
		Settings.IsDisabled = true;
	}
}
