using System;
using Kingmaker.Sound.Base;

namespace Kingmaker.UI.Sound.Base;

public class VoiceOverStatus
{
	private const double DelayTimeSec = 0.25;

	private TimeSpan m_StartTime;

	private bool m_Play;

	public uint PlayingSoundId { get; set; }

	public bool IsEnded
	{
		get
		{
			if (m_Play)
			{
				return Game.Instance.Player.RealTime > m_StartTime;
			}
			return false;
		}
	}

	public VoiceOverStatus(TimeSpan startTime)
	{
		m_StartTime = startTime + TimeSpan.FromSeconds(0.25);
		m_Play = false;
	}

	public void HandleCallback(object cookie, AkCallbackType type, object info)
	{
		if (type == AkCallbackType.AK_Duration)
		{
			AkDurationCallbackInfo akDurationCallbackInfo = (AkDurationCallbackInfo)info;
			m_StartTime += TimeSpan.FromMilliseconds(akDurationCallbackInfo.fDuration);
			m_Play = true;
		}
	}

	public void Stop()
	{
		SoundEventsManager.StopPlayingById(PlayingSoundId);
	}
}
