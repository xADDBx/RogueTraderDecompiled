using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers.FX;

public class CameraFXSoundController : IController
{
	public SFXWrapper CurrentWrapper;

	private Dictionary<SoundPriority, List<SFXWrapper>> m_Events = new Dictionary<SoundPriority, List<SFXWrapper>>();

	public void TryStartEvent(SFXWrapper wrapper)
	{
		if (!m_Events.ContainsKey(wrapper.Priority))
		{
			m_Events[wrapper.Priority] = new List<SFXWrapper>();
		}
		m_Events[wrapper.Priority].Add(wrapper);
		if (CurrentWrapper != null && CurrentWrapper != wrapper)
		{
			if (CurrentWrapper.Priority <= wrapper.Priority)
			{
				CurrentWrapper.StopEvent();
				CurrentWrapper = null;
				StartEvent(wrapper);
			}
		}
		else if (CurrentWrapper == null)
		{
			StartEvent(wrapper);
		}
	}

	public void TryStopEvent(SFXWrapper wrapper)
	{
		if (!m_Events.TryGetValue(wrapper.Priority, out var value))
		{
			return;
		}
		value.Remove(wrapper);
		if (CurrentWrapper == null || CurrentWrapper == wrapper)
		{
			if (CurrentWrapper == wrapper)
			{
				CurrentWrapper.StopEvent();
				CurrentWrapper = null;
			}
			if (TryFindHigherPriorityEventThan(SoundPriority.None, out var result))
			{
				StartEvent(result);
			}
		}
	}

	private void StartEvent(SFXWrapper wrapper)
	{
		CurrentWrapper = wrapper;
		wrapper.StartEvent();
	}

	private bool TryFindHigherPriorityEventThan(SoundPriority priority, out SFXWrapper result)
	{
		result = null;
		SoundPriority soundPriority = priority;
		foreach (KeyValuePair<SoundPriority, List<SFXWrapper>> @event in m_Events)
		{
			if (@event.Value.Count != 0 && @event.Key > soundPriority)
			{
				soundPriority = @event.Key;
				result = @event.Value.LastOrDefault();
			}
		}
		return result != null;
	}
}
