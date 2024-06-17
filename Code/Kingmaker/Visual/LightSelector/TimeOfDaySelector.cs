using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.LightSelector;

public class TimeOfDaySelector : MonoBehaviour, ITimeOfDayChangedHandler, ISubscriber
{
	[SerializeField]
	[FormerlySerializedAs("Settings")]
	private BlueprintTimeOfDaySettingsReference m_Settings;

	private GameObject m_LightInstance;

	public BlueprintTimeOfDaySettings Settings => m_Settings?.Get();

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		ChooseLight();
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnTimeOfDayChanged()
	{
		ChooseLight();
	}

	private void ChooseLight()
	{
		if ((bool)Settings)
		{
			if ((bool)m_LightInstance)
			{
				Object.Destroy(m_LightInstance);
				m_LightInstance = null;
			}
			GameObject gameObject = null;
			BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
			BlueprintTimeOfDaySettings settings = Settings;
			switch (currentlyLoadedAreaPart?.GetTimeOfDay() ?? Game.Instance.TimeOfDay)
			{
			case TimeOfDay.Morning:
				gameObject = settings.Morning;
				break;
			case TimeOfDay.Day:
				gameObject = settings.Day;
				break;
			case TimeOfDay.Evening:
				gameObject = settings.Evening;
				break;
			case TimeOfDay.Night:
				gameObject = settings.Night;
				break;
			}
			if (gameObject != null)
			{
				m_LightInstance = Object.Instantiate(gameObject, base.transform, worldPositionStays: false);
			}
		}
	}
}
