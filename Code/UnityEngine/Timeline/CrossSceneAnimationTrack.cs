using Kingmaker.Utility.Attributes;
using UnityEngine.Playables;

namespace UnityEngine.Timeline;

public class CrossSceneAnimationTrack : AnimationTrack, ICrossSceneTrack
{
	[SerializeField]
	[InspectorReadOnly]
	private string _bindKey = string.Empty;

	public string BindKey => _bindKey;

	public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		base.GatherProperties(director, driver);
	}
}
