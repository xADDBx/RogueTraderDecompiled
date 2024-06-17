using Kingmaker.Visual.DayNightCycle;
using UnityEngine;

namespace Kingmaker;

public class CharacterStudioLightApplySceneRendreringSettings : MonoBehaviour
{
	public GameObject lightController;

	public SceneLightConfig config;

	private LightController lightControllerScript;

	private void Awake()
	{
		Debug.Log("Awake");
		lightControllerScript = lightController.GetComponent<LightController>();
	}

	private void Start()
	{
		Debug.Log("Start");
		lightControllerScript.ApplySceneRendreringSettings(config);
	}
}
