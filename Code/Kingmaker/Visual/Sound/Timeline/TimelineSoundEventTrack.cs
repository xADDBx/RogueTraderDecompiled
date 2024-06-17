using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.Visual.Sound.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(TimelineSoundEventPlayable))]
[TrackBindingType(typeof(GameObject))]
public class TimelineSoundEventTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<TimelineSoundEventPlayableBehavior> scriptPlayable = ScriptPlayable<TimelineSoundEventPlayableBehavior>.Create(graph);
		scriptPlayable.SetInputCount(inputCount);
		foreach (TimelineClip clip in GetClips())
		{
			(clip.asset as TimelineSoundEventPlayable).owningClip = clip;
		}
		return scriptPlayable;
	}
}
