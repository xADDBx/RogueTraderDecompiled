using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class BarkPlayableAsset : PlayableAsset, ITimelineClipAsset
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark, GetNameFromAsset = true)]
	[ValidateNotNull]
	public SharedStringAsset Bark;

	public bool AutoDuration;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
	{
		ScriptPlayable<BarkPlayableBehaviour> scriptPlayable = ScriptPlayable<BarkPlayableBehaviour>.Create(graph);
		PlayableDirector component = go.GetComponent<PlayableDirector>();
		TrackAsset trackAsset = (component.playableAsset as TimelineAsset).GetOutputTracks().Single((TrackAsset t) => t.GetClips().Any((TimelineClip c) => c.asset == this));
		GameObject gameObject = component.GetGenericBinding(trackAsset) as GameObject;
		if (trackAsset == null)
		{
			PFLog.Default.Error($"Cannot find track for clip {this} on {go}");
			return Playable.Null;
		}
		if (gameObject == null)
		{
			PFLog.Default.Error($"Cannot find bound object for clip {this} on {go} track {trackAsset}");
			return Playable.Null;
		}
		scriptPlayable.GetBehaviour().Owner = (gameObject ? gameObject.GetComponentInParent<UnitEntityView>() : null);
		scriptPlayable.GetBehaviour().SharedText = Bark;
		BarkPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		float num;
		if (!AutoDuration)
		{
			num = (float)duration;
		}
		else
		{
			LocalizedString localizedString = Bark?.String;
			num = UIUtility.GetBarkDuration((localizedString != null) ? ((string)localizedString) : "");
		}
		behaviour.Duration = num;
		PFLog.Default.Log($"Playablebark: {scriptPlayable.GetBehaviour().Owner} says {scriptPlayable.GetBehaviour().SharedText.NameSafe()}");
		return scriptPlayable;
	}
}
