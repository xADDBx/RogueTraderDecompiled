using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Enums.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[Serializable]
[CreateAssetMenu(fileName = "AnimationClipWithEvents", menuName = "Animation Manager/Animation Clip with Events")]
public class AnimationClipWrapper : ScriptableObject
{
	private enum RecognizedEventNames
	{
		PostEvent = 0,
		PostEventMapped = 1,
		PostMainWeaponEquipEvent = 4,
		PostOffWeaponEquipEvent = 5,
		PostMainWeaponUnequipEvent = 6,
		PostOffWeaponUnequipEvent = 7,
		PostArmorFoleyEvent = 8,
		PostEventWithSurface = 9,
		AnimateWeaponTrail = 10,
		PostCommandActEvent = 11,
		PostEventWithPrefix = 12,
		PostDecoratorObject = 13,
		PlayFootstep = 14,
		PlayBodyfall = 15,
		FxAnimatorToggleAction = 16,
		HideTorchEvent = 17,
		UnhideTorchEvent = 18
	}

	[SerializeField]
	private AnimationClip m_AnimationClip;

	[SerializeField]
	private List<AnimationClipEventTrack> m_EventTracks = new List<AnimationClipEventTrack>();

	[NonSerialized]
	private AnimationClipEvent[] _EventsSorted;

	public AnimationClip AnimationClip
	{
		get
		{
			return m_AnimationClip;
		}
		set
		{
			m_AnimationClip = value;
		}
	}

	public List<AnimationClipEventTrack> EventTracks
	{
		get
		{
			return m_EventTracks;
		}
		set
		{
			m_EventTracks = value;
		}
	}

	public float Length
	{
		get
		{
			if (!(AnimationClip != null))
			{
				return 0f;
			}
			return AnimationClip.length;
		}
	}

	public bool IsLooping
	{
		get
		{
			if (!(AnimationClip != null))
			{
				return false;
			}
			return AnimationClip.isLooping;
		}
	}

	public IEnumerable<AnimationClipEvent> Events => from _eventTrack in EventTracks.SelectMany(delegate(AnimationClipEventTrack _eventTrack)
		{
			if (_eventTrack == null)
			{
				PFLog.Default.Error("Event track is null!");
			}
			return (_eventTrack?.Events).EmptyIfNull();
		})
		where _eventTrack != null
		select _eventTrack;

	public AnimationClipEvent[] EventsSorted
	{
		get
		{
			if (_EventsSorted == null)
			{
				_EventsSorted = Events.ToArray();
				if (_EventsSorted.Any())
				{
					Array.Sort(_EventsSorted, (AnimationClipEvent _event1, AnimationClipEvent _event2) => (int)((_event1.Time - _event2.Time) * 1000f));
				}
			}
			return _EventsSorted;
		}
	}

	public AnimationClipWrapper(AnimationClip animationClip, IEnumerable<AnimationClipEventTrack> animationSoundTracks = null)
	{
		m_AnimationClip = animationClip;
		m_EventTracks = ((animationSoundTracks != null) ? new List<AnimationClipEventTrack>(animationSoundTracks) : new List<AnimationClipEventTrack>());
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", m_AnimationClip, string.Join(", ", m_EventTracks.Select((AnimationClipEventTrack _track) => (!(_track != null)) ? "" : _track.ToString())));
	}

	private static AnimationClipEvent GetAnimationClipEvent(AnimationEvent animationEvent)
	{
		if (animationEvent == null)
		{
			throw new ArgumentNullException("animationEvent");
		}
		if (!Enum.TryParse<RecognizedEventNames>(animationEvent.functionName, ignoreCase: false, out var result))
		{
			throw new NotSupportedException("Animation event of type " + animationEvent.functionName + " is not supported.");
		}
		return result switch
		{
			RecognizedEventNames.PostEvent => new AnimationClipEventSound(animationEvent.time, isLooped: false, animationEvent.stringParameter, animationEvent.stringParameter, 1f), 
			RecognizedEventNames.PostEventWithPrefix => new AnimationClipEventSoundWithPrefix(animationEvent.time, isLooped: false, animationEvent.stringParameter, 1f), 
			RecognizedEventNames.PostEventMapped => new AnimationClipEventSoundMapped(animationEvent.time, (MappedAnimationEventType)animationEvent.intParameter), 
			RecognizedEventNames.PostMainWeaponEquipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.MainWeaponEquip), 
			RecognizedEventNames.PostOffWeaponEquipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.OffWeaponEquip), 
			RecognizedEventNames.PostMainWeaponUnequipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.MainWeaponUnequip), 
			RecognizedEventNames.PostOffWeaponUnequipEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.OffWeaponUnequip), 
			RecognizedEventNames.PostArmorFoleyEvent => new AnimationClipEventSoundUnit(animationEvent.time, AnimationClipEventSoundUnit.SoundType.ArmorFoley), 
			RecognizedEventNames.PostEventWithSurface => new AnimationClipEventSoundSurface(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PlayBodyfall => new AnimationClipEventBodyFall(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PlayFootstep => new AnimationClipEventFootStep(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.AnimateWeaponTrail => new AnimationClipEventAnimateWeaponTrail(animationEvent.time, animationEvent.floatParameter), 
			RecognizedEventNames.FxAnimatorToggleAction => new AnimationClipEventToggleFxAnimator(animationEvent.time, animationEvent.stringParameter), 
			RecognizedEventNames.PostCommandActEvent => new AnimationClipEventAct(animationEvent.time), 
			RecognizedEventNames.PostDecoratorObject => new AnimationClipEventDecoratorObject(animationEvent.time, animationEvent.objectReferenceParameter as UnitAnimationDecoratorObject), 
			RecognizedEventNames.HideTorchEvent => new AnimationClipEventPlaceFootprint(animationEvent.time, animationEvent.stringParameter, animationEvent.intParameter), 
			RecognizedEventNames.UnhideTorchEvent => new AnimationClipEventTorchShow(animationEvent.time), 
			_ => throw new NotSupportedException($"Animation event of type {result} is not supported."), 
		};
	}
}
