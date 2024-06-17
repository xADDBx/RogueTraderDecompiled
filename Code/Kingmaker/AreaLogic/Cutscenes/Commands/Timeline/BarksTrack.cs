using UnityEngine;
using UnityEngine.Timeline;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

[TrackBindingType(typeof(GameObject))]
[TrackClipType(typeof(BarkPlayableAsset))]
[TrackColor(1f, 1f, 0f)]
public class BarksTrack : PlayableTrack
{
}
