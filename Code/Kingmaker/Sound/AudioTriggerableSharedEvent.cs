using System.Collections.Generic;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

internal class AudioTriggerableSharedEvent : AkAudioTriggerable
{
	private class SharedEventData
	{
		public bool IsPlaying;

		public bool PositionsUpdated;

		public readonly List<AudioTriggerableSharedEvent> List = new List<AudioTriggerableSharedEvent>();

		public void FinishedPlaying(object in_cookie, AkCallbackType in_type, object in_info)
		{
			IsPlaying = false;
		}

		public void DoTrigger(AkEventReference evt)
		{
			if (List.Count == 0 || IsPlaying)
			{
				return;
			}
			if (!PositionsUpdated)
			{
				AkPositionArray akPositionArray = new AkPositionArray((uint)List.Count);
				for (int i = 0; i < List.Count; i++)
				{
					akPositionArray.Add(List[i].transform.position, List[i].transform.forward, List[i].transform.up);
				}
				AkSoundEngine.SetMultiplePositions(List[0].gameObject, akPositionArray, (ushort)akPositionArray.Count, AkMultiPositionType.MultiPositionType_MultiSources);
				PositionsUpdated = true;
			}
			SoundEventsManager.PostEvent(evt.ValueHash, List[0].gameObject, 1u, FinishedPlaying, null);
		}
	}

	private class SharedEventsCache : RegisteredObjectBase
	{
		public readonly Dictionary<string, SharedEventData> EventToData = new Dictionary<string, SharedEventData>();

		public static SharedEventsCache Instance
		{
			get
			{
				SharedEventsCache sharedEventsCache = ObjectRegistry<SharedEventsCache>.Instance.MaybeSingle;
				if (sharedEventsCache == null)
				{
					sharedEventsCache = new SharedEventsCache();
					sharedEventsCache.Enable();
				}
				return sharedEventsCache;
			}
		}
	}

	[SerializeField]
	private AkEventReference m_Event;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!SharedEventsCache.Instance.EventToData.TryGetValue(m_Event.Value, out var value))
		{
			value = (SharedEventsCache.Instance.EventToData[m_Event.Value] = new SharedEventData());
		}
		value.List.Add(this);
		value.PositionsUpdated = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (SharedEventsCache.Instance.EventToData.TryGetValue(m_Event.Value, out var value))
		{
			value.List.Remove(this);
			value.PositionsUpdated = false;
		}
	}

	public override void OnTrigger()
	{
		if (SharedEventsCache.Instance.EventToData.TryGetValue(m_Event.Value, out var value) && !value.IsPlaying)
		{
			value.DoTrigger(m_Event);
		}
	}

	protected override void OnStop(int fade)
	{
		m_Event.ExecuteAction(base.gameObject, AkActionOnEventType.AkActionOnEventType_Stop, fade, AkCurveInterpolation.AkCurveInterpolation_Linear);
	}
}
