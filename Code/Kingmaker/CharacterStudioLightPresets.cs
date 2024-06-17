using System.Collections.Generic;
using Kingmaker.Visual.DayNightCycle;
using UnityEngine;

namespace Kingmaker;

public class CharacterStudioLightPresets : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> lightPresets = new List<GameObject>();

	[SerializeField]
	private List<SceneLightConfig> lightConfigs = new List<SceneLightConfig>();

	public SceneLightConfig configPres;

	private int m_index;

	private LightController lightControllerScript;

	public void SetActiveLightPreset(int index)
	{
		for (int i = 0; i < lightPresets.Count; i++)
		{
			lightPresets[i].SetActive(index == i);
			m_index = index;
			configPres = lightConfigs[m_index];
		}
	}

	public int GetLightPresetsCount()
	{
		return lightPresets.Count;
	}
}
