using Kingmaker.Visual.DayNightCycle;
using UnityEngine;

namespace Kingmaker;

public class CharacterStudioLightManager : MonoBehaviour
{
	public GameObject m_DollRoom;

	public GameObject m_LightPresets;

	private GameObject m_InstDollRoom;

	private GameObject m_InstLightPresets;

	[Space(10f)]
	public GameObject lightController;

	private LightController lightControllerScript;

	public SceneLightConfig configInv;

	private CharacterStudioLightPresets m_characterStudioLightPresets;

	public Transform buttonsContainer;

	public CharacterStudioLightButtonTemplate buttonTemplate;

	private void Awake()
	{
		m_InstDollRoom = Object.Instantiate(m_DollRoom, new Vector3(0f, -0.54f, 5.6f), base.transform.rotation);
		m_InstLightPresets = Object.Instantiate(m_LightPresets, base.transform.position, base.transform.rotation);
		m_InstDollRoom.SetActive(value: true);
		m_InstLightPresets.SetActive(value: false);
		lightControllerScript = lightController.GetComponent<LightController>();
		lightControllerScript.ApplySceneRendreringSettings(configInv);
		m_characterStudioLightPresets = m_InstLightPresets.GetComponent<CharacterStudioLightPresets>();
		CreatePresetsButtons();
	}

	public void DollRoomInstantiate()
	{
		if (!m_InstDollRoom.activeSelf)
		{
			m_InstLightPresets.SetActive(value: false);
			m_InstDollRoom.SetActive(value: true);
			lightControllerScript.ApplySceneRendreringSettings(configInv);
		}
	}

	public void SelectPreset(int index)
	{
		m_characterStudioLightPresets.SetActiveLightPreset(index);
		m_InstDollRoom.SetActive(value: false);
		m_InstLightPresets.SetActive(value: true);
		lightControllerScript.ApplySceneRendreringSettings(m_characterStudioLightPresets.configPres);
	}

	private void CreatePresetsButtons()
	{
		int lightPresetsCount = m_characterStudioLightPresets.GetLightPresetsCount();
		for (int i = 0; i < lightPresetsCount; i++)
		{
			Object.Instantiate(buttonTemplate, buttonsContainer).LinkButton(i, this);
		}
	}
}
