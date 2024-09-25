using System;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Sound.Timeline;

public class TimelineSoundEventPlayableBehavior : PlayableBehaviour
{
	[Flags]
	private enum Actions
	{
		None = 0,
		Playback = 1,
		Retrigger = 2,
		DelayedStop = 4,
		Seek = 8,
		FadeIn = 0x10,
		FadeOut = 0x20
	}

	private static readonly LogChannel Logger = LogChannel.Audio;

	private float m_CurrentDuration = -1f;

	private float m_CurrentDurationProportion = 1f;

	private bool m_EventIsPlaying;

	private bool m_FadeinTriggered;

	private bool m_FadeoutTriggered;

	private float m_PreviousEventStartTime;

	private const uint CallbackFlags = 9u;

	private Actions m_RequiredActions;

	private const int ScrubPlaybackLengthMs = 100;

	public string EventName;

	public float eventDurationMax;

	public float eventDurationMin;

	public float blendInDuration;

	public float blendOutDuration;

	public float easeInDuration;

	public float easeOutDuration;

	public AkCurveInterpolation blendInCurve;

	public AkCurveInterpolation blendOutCurve;

	public GameObject eventObject;

	public bool retriggerEvent;

	private bool m_WasScrubbingAndRequiresRetrigger;

	public bool StopEventAtClipEnd;

	public bool PrintDebugInformation;

	private void CallbackHandler(object inCookie, AkCallbackType inType, AkCallbackInfo inInfo)
	{
		switch (inType)
		{
		case AkCallbackType.AK_EndOfEvent:
			m_EventIsPlaying = (m_FadeinTriggered = (m_FadeoutTriggered = false));
			break;
		case AkCallbackType.AK_Duration:
		{
			float fEstimatedDuration = ((AkDurationCallbackInfo)inInfo).fEstimatedDuration;
			m_CurrentDuration = fEstimatedDuration * m_CurrentDurationProportion / 1000f;
			break;
		}
		}
	}

	private bool IsScrubbing(Playable playable, FrameData info)
	{
		double previousTime = playable.GetPreviousTime();
		double time = playable.GetTime();
		Math.Abs(time - previousTime);
		if (previousTime == 0.0)
		{
			return time > 0.0;
		}
		return false;
	}

	private void PrintInfo(string functionName, Playable playable, FrameData info)
	{
		if (PrintDebugInformation)
		{
			double previousTime = playable.GetPreviousTime();
			double time = playable.GetTime();
			double num = Math.Abs(time - previousTime);
			UnityEngine.Debug.Log($"{functionName}: prevTime={previousTime}; curTime={time}; computedDelta={num}; evalType={info.evaluationType}; deltaTime={info.deltaTime}; playState={info.effectivePlayState}; timeHeld={info.timeHeld}; speed={info.effectiveSpeed}; parentSpeed={info.effectiveParentSpeed}");
		}
	}

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		base.PrepareFrame(playable, info);
		PrintInfo("PrepareFrame", playable, info);
		if (EventName == null)
		{
			return;
		}
		bool flag = ShouldPlay(playable);
		if (IsScrubbing(playable, info) && flag)
		{
			m_RequiredActions |= Actions.Seek;
			if (!m_EventIsPlaying)
			{
				m_RequiredActions |= Actions.Playback;
				CheckForFadeInFadeOut(playable);
			}
		}
		else if (!m_EventIsPlaying && (m_RequiredActions & Actions.Playback) == 0)
		{
			m_RequiredActions |= Actions.Retrigger;
			CheckForFadeInFadeOut(playable);
		}
		else
		{
			CheckForFadeOut(playable, playable.GetTime());
		}
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		PrintInfo("OnBehaviourPlay", playable, info);
		base.OnBehaviourPlay(playable, info);
		if (EventName != null && ShouldPlay(playable))
		{
			m_RequiredActions |= Actions.Playback;
			if (IsScrubbing(playable, info))
			{
				m_WasScrubbingAndRequiresRetrigger = true;
			}
			else if (GetProportionalTime(playable) > 0.05f)
			{
				m_RequiredActions |= Actions.Seek;
			}
			CheckForFadeInFadeOut(playable);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		PrintInfo("OnBehaviourPause", playable, info);
		m_WasScrubbingAndRequiresRetrigger = false;
		base.OnBehaviourPause(playable, info);
		if (eventObject != null && EventName != null && StopEventAtClipEnd)
		{
			StopEvent();
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		PrintInfo("ProcessFrame", playable, info);
		base.ProcessFrame(playable, info, playerData);
		if (EventName == null)
		{
			return;
		}
		GameObject gameObject = playerData as GameObject;
		if (gameObject != null)
		{
			eventObject = gameObject;
		}
		if (!(eventObject == null))
		{
			if ((m_RequiredActions & Actions.Playback) != 0)
			{
				PlayEvent();
			}
			if ((m_RequiredActions & Actions.Seek) != 0)
			{
				SeekToTime(playable);
			}
			if ((retriggerEvent || m_WasScrubbingAndRequiresRetrigger) && (m_RequiredActions & Actions.Retrigger) != 0)
			{
				RetriggerEvent(playable);
			}
			if ((m_RequiredActions & Actions.DelayedStop) != 0)
			{
				StopEvent(100);
			}
			if (!m_FadeinTriggered && (m_RequiredActions & Actions.FadeIn) != 0)
			{
				TriggerFadeIn(playable);
			}
			if (!m_FadeoutTriggered && (m_RequiredActions & Actions.FadeOut) != 0)
			{
				TriggerFadeOut(playable);
			}
			m_RequiredActions = Actions.None;
		}
	}

	private bool ShouldPlay(Playable playable)
	{
		playable.GetPreviousTime();
		double time = playable.GetTime();
		if (retriggerEvent)
		{
			return true;
		}
		if (eventDurationMax == eventDurationMin && eventDurationMin != -1f)
		{
			return time < (double)eventDurationMax;
		}
		time -= (double)m_PreviousEventStartTime;
		float num = ((m_CurrentDuration == -1f) ? ((float)playable.GetDuration()) : m_CurrentDuration);
		return time < (double)num;
	}

	private void CheckForFadeInFadeOut(Playable playable)
	{
		double time = playable.GetTime();
		if ((double)blendInDuration > time || (double)easeInDuration > time)
		{
			m_RequiredActions |= Actions.FadeIn;
		}
		CheckForFadeOut(playable, time);
	}

	private void CheckForFadeOut(Playable playable, double currentClipTime)
	{
		double num = playable.GetDuration() - currentClipTime;
		if ((double)blendOutDuration >= num || (double)easeOutDuration >= num)
		{
			m_RequiredActions |= Actions.FadeOut;
		}
	}

	private void TriggerFadeIn(Playable playable)
	{
		double time = playable.GetTime();
		double num = (double)Mathf.Max(easeInDuration, blendInDuration) - time;
		if (num > 0.0)
		{
			m_FadeinTriggered = true;
			AKRESULT result = AkSoundEngine.ExecuteActionOnEvent(EventName, AkActionOnEventType.AkActionOnEventType_Pause, eventObject, 0, blendOutCurve);
			Verify(result);
			result = AkSoundEngine.ExecuteActionOnEvent(EventName, AkActionOnEventType.AkActionOnEventType_Resume, eventObject, (int)(num * 1000.0), blendInCurve);
			Verify(result);
		}
	}

	private void TriggerFadeOut(Playable playable)
	{
		m_FadeoutTriggered = true;
		double num = playable.GetDuration() - playable.GetTime();
		AKRESULT result = AkSoundEngine.ExecuteActionOnEvent(EventName, AkActionOnEventType.AkActionOnEventType_Stop, eventObject, (int)(num * 1000.0), blendOutCurve);
		Verify(result);
	}

	private void StopEvent(int transition = 0)
	{
		if (m_EventIsPlaying)
		{
			AKRESULT result = AkSoundEngine.ExecuteActionOnEvent(EventName, AkActionOnEventType.AkActionOnEventType_Stop, eventObject, transition, AkCurveInterpolation.AkCurveInterpolation_Linear);
			Verify(result);
		}
	}

	private bool PostEvent()
	{
		m_FadeinTriggered = (m_FadeoutTriggered = false);
		uint num = SoundEventsManager.PostEvent(EventName, eventObject, 9u, CallbackHandler, null);
		m_EventIsPlaying = num != 0;
		return m_EventIsPlaying;
	}

	private void PlayEvent()
	{
		if (PostEvent())
		{
			m_CurrentDurationProportion = 1f;
			m_PreviousEventStartTime = 0f;
		}
	}

	private void RetriggerEvent(Playable playable)
	{
		m_WasScrubbingAndRequiresRetrigger = false;
		if (PostEvent())
		{
			m_CurrentDurationProportion = 1f - SeekToTime(playable);
			m_PreviousEventStartTime = (float)playable.GetTime();
		}
	}

	private float GetProportionalTime(Playable playable)
	{
		if (eventDurationMax == eventDurationMin && eventDurationMin != -1f)
		{
			return (float)playable.GetTime() % eventDurationMax / eventDurationMax;
		}
		float num = (float)playable.GetTime() - m_PreviousEventStartTime;
		float num2 = ((m_CurrentDuration == -1f) ? ((float)playable.GetDuration()) : m_CurrentDuration);
		return num % num2 / num2;
	}

	private float SeekToTime(Playable playable)
	{
		float proportionalTime = GetProportionalTime(playable);
		if (proportionalTime >= 1f)
		{
			return 1f;
		}
		if (m_EventIsPlaying)
		{
			AkSoundEngine.SeekOnEvent(EventName, eventObject, proportionalTime);
		}
		return proportionalTime;
	}

	private void Verify(AKRESULT result)
	{
		if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
		{
			Logger.Warning("Unsuccessful call made on {0}.", GetType().Name);
		}
	}

	private void VerifyPlayingID(uint playingId)
	{
		if (playingId == 0 && AkSoundEngine.IsInitialized())
		{
			Logger.Error("WwiseUnity: Could not post event (name: " + EventName + "). Please make sure to load or rebuild the appropriate SoundBank.");
		}
	}
}
