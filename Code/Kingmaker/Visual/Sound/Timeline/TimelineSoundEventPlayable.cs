using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.Visual.Sound.Timeline;

public class TimelineSoundEventPlayable : PlayableAsset, ITimelineClipAsset
{
	public string SoundName = "";

	[SerializeField]
	private AkCurveInterpolation blendInCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

	[SerializeField]
	private AkCurveInterpolation blendOutCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public float eventDurationMax = -1f;

	public float eventDurationMin = -1f;

	[NonSerialized]
	public TimelineClip owningClip;

	[SerializeField]
	private bool retriggerEvent;

	public bool UseWwiseEventDuration = true;

	public bool PrintDebugInformation;

	[SerializeField]
	private bool StopEventAtClipEnd = true;

	ClipCaps ITimelineClipAsset.clipCaps => ClipCaps.Looping | ClipCaps.Blending;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<TimelineSoundEventPlayableBehavior> scriptPlayable = ScriptPlayable<TimelineSoundEventPlayableBehavior>.Create(graph);
		if (SoundName == null)
		{
			return scriptPlayable;
		}
		TimelineSoundEventPlayableBehavior behaviour = scriptPlayable.GetBehaviour();
		behaviour.EventName = SoundName;
		behaviour.blendInCurve = blendInCurve;
		behaviour.blendOutCurve = blendOutCurve;
		behaviour.PrintDebugInformation = PrintDebugInformation;
		if (owningClip != null)
		{
			behaviour.easeInDuration = (float)owningClip.easeInDuration;
			behaviour.easeOutDuration = (float)owningClip.easeOutDuration;
			behaviour.blendInDuration = (float)owningClip.blendInDuration;
			behaviour.blendOutDuration = (float)owningClip.blendOutDuration;
		}
		else
		{
			behaviour.easeInDuration = (behaviour.easeOutDuration = (behaviour.blendInDuration = (behaviour.blendOutDuration = 0f)));
		}
		behaviour.retriggerEvent = retriggerEvent;
		behaviour.StopEventAtClipEnd = StopEventAtClipEnd;
		behaviour.eventObject = owner;
		behaviour.eventDurationMin = eventDurationMin;
		behaviour.eventDurationMax = eventDurationMax;
		return scriptPlayable;
	}
}
