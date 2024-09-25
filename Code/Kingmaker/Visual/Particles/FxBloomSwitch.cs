using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxBloomSwitch : MonoBehaviour
{
	public GameObject ShowIfBloomEnabled;

	public GameObject ShowIfBloomDisabled;

	private void OnEnable()
	{
		UpdateVisibility();
	}

	private void Update()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		SetBloomEnabled(enabled: false);
	}

	private void SetBloomEnabled(bool enabled)
	{
		if (ShowIfBloomEnabled != null && ShowIfBloomEnabled.activeSelf != enabled)
		{
			ShowIfBloomEnabled.SetActive(enabled);
		}
		if (ShowIfBloomDisabled != null && ShowIfBloomDisabled.activeSelf == enabled)
		{
			ShowIfBloomDisabled.SetActive(!enabled);
		}
	}
}
