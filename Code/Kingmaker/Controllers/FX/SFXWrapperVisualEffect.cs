using Kingmaker.Blueprints.Root;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Controllers.FX;

public class SFXWrapperVisualEffect : SFXWrapper
{
	private PostProcessingEffectsLibrary.SoundEventReferences m_SoundEventReferences;

	public override SoundPriority Priority => SoundPriority.PlayerConditions;

	public SFXWrapperVisualEffect(PostProcessingEffectsLibrary.SoundEventReferences soundEventReferences)
	{
		m_SoundEventReferences = soundEventReferences;
	}

	public override void StartEvent()
	{
		AkSoundEngine.SetState(m_SoundEventReferences.State.Group, m_SoundEventReferences.State.Value);
		VFXWeatherSystem instance = VFXWeatherSystem.Instance;
		GameObject gameObject = ((instance != null) ? instance.gameObject : null);
		SoundEventsManager.PostEvent(m_SoundEventReferences.StartEventName, gameObject);
	}

	public override void StopEvent()
	{
		AkSoundEngine.SetState(m_SoundEventReferences.State.Group, "None");
		VFXWeatherSystem instance = VFXWeatherSystem.Instance;
		GameObject gameObject = ((instance != null) ? instance.gameObject : null);
		SoundEventsManager.PostEvent(m_SoundEventReferences.StopEventName, gameObject);
	}

	public override string ToString()
	{
		return m_SoundEventReferences.StartEventName;
	}
}
