using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Blueprints;

public class ClipEventExternalReceiver : MonoBehaviour
{
	[Serializable]
	public class IDEventPair
	{
		public int Id;

		[SerializeReference]
		public AnimationClipEvent Event;
	}

	public Dictionary<Type, ClipEventType> s_EventTypeToName = new Dictionary<Type, ClipEventType>
	{
		{
			typeof(AnimationClipEventBodyFall),
			ClipEventType.BodyFall
		},
		{
			typeof(AnimationClipEventDecoratorObject),
			ClipEventType.DecoratorObject
		},
		{
			typeof(AnimationClipEventFootStep),
			ClipEventType.FootStep
		},
		{
			typeof(AnimationClipEventAct),
			ClipEventType.Act
		},
		{
			typeof(AnimationClipEventSound),
			ClipEventType.Sound
		},
		{
			typeof(AnimationClipEventSoundMapped),
			ClipEventType.SoundMapped
		},
		{
			typeof(AnimationClipEventSoundSurface),
			ClipEventType.SoundSurface
		},
		{
			typeof(AnimationClipEventSoundUnit),
			ClipEventType.SoundUnit
		},
		{
			typeof(AnimationClipEventSoundWithPrefix),
			ClipEventType.SoundWithPrefix
		},
		{
			typeof(AnimationClipEventToggleFxAnimator),
			ClipEventType.ToggleFxAnimator
		},
		{
			typeof(AnimationClipEventPlaceFootprint),
			ClipEventType.PlaceFootprint
		},
		{
			typeof(AnimationClipEventTorchShow),
			ClipEventType.TorchShow
		}
	};

	[SerializeReference]
	public List<IDEventPair> m_Events = new List<IDEventPair>();

	public List<IDEventPair> Events
	{
		get
		{
			return m_Events;
		}
		set
		{
			m_Events = ((value != null) ? new List<IDEventPair>(value) : null);
		}
	}

	public void StartEvent(ClipEventType clipEventType, AnimationManager animationManager, int id)
	{
		Type key = s_EventTypeToName.Select((KeyValuePair<Type, ClipEventType> x) => x).FirstOrDefault((KeyValuePair<Type, ClipEventType> x) => x.Value == clipEventType).Key;
		foreach (IDEventPair @event in Events)
		{
			if (@event != null && key != null && @event.Event.GetType() == key && @event.Id == id)
			{
				@event.Event.Start(animationManager);
			}
		}
	}
}
