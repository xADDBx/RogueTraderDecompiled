using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Sound.Base;

public class SoundEventsManager
{
	private List<uint> m_PlayingIdsToStop = new List<uint>();

	public static SoundEventsManager Instance { get; }

	private bool StopAllEvents { get; set; }

	static SoundEventsManager()
	{
		Instance = new SoundEventsManager();
	}

	private SoundEventsManager()
	{
	}

	public static uint PostEvent(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		return Instance.PostEventInternal(eventName, gameObject, flags, callback, cookie);
	}

	public static uint PostEvent(string eventName, GameObject gameObject, bool canBeStopped = false)
	{
		return Instance.PostEventInternal(eventName, gameObject, canBeStopped);
	}

	public static uint PostEvent(uint eventId, GameObject gameObject)
	{
		return Instance.PostEventInternal(eventId, gameObject);
	}

	public static uint PostEvent(uint eventId, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		return Instance.PostEventInternal(eventId, gameObject, flags, callback, cookie);
	}

	public static void StopPlayingById(uint playingId)
	{
		Instance.StopPlayingByIdInternal(playingId);
	}

	public void Update()
	{
		if (!StopAllEvents)
		{
			return;
		}
		foreach (uint item in m_PlayingIdsToStop)
		{
			AkSoundEngine.StopPlayingID(item, 0);
		}
		m_PlayingIdsToStop.Clear();
	}

	public void SetStoppingAllState(bool active)
	{
		StopAllEvents = active;
		if (StopAllEvents)
		{
			AkSoundEngine.PostEvent("TECH_SkipCutscene_StopAudio", null);
		}
		else
		{
			AkSoundEngine.PostEvent("TECH_SkipCutscene_ResumeAudio", null);
		}
		m_PlayingIdsToStop.Clear();
	}

	private uint PostEventInternal(string eventName, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		uint id = AkSoundEngine.PostEvent(eventName, gameObject, flags, callback, cookie);
		return SaveIfNeeded(id);
	}

	private uint PostEventInternal(string eventName, GameObject gameObject, bool canBeStopped = false)
	{
		uint id = AkSoundEngine.PostEvent(eventName, gameObject);
		return SaveIfNeeded(id, canBeStopped);
	}

	private uint PostEventInternal(uint eventId, GameObject gameObject)
	{
		uint id = AkSoundEngine.PostEvent(eventId, gameObject);
		return SaveIfNeeded(id);
	}

	private uint PostEventInternal(uint eventId, GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie)
	{
		uint id = AkSoundEngine.PostEvent(eventId, gameObject, flags, callback, cookie);
		return SaveIfNeeded(id);
	}

	private uint SaveIfNeeded(uint id, bool canBeStopped = false)
	{
		if (StopAllEvents && canBeStopped)
		{
			m_PlayingIdsToStop.Add(id);
		}
		return id;
	}

	private void StopPlayingByIdInternal(uint playingId)
	{
		AkSoundEngine.StopPlayingID(playingId);
		m_PlayingIdsToStop.RemoveAll((uint x) => x == playingId);
	}
}
