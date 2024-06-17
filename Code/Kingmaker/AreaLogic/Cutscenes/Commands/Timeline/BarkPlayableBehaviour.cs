using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Localization;
using Kingmaker.View;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class BarkPlayableBehaviour : IPlayableBehaviour
{
	public UnitEntityView Owner { get; set; }

	public SharedStringAsset SharedText { get; set; }

	public float Duration { get; set; }

	public void OnGraphStart(Playable playable)
	{
	}

	public void OnGraphStop(Playable playable)
	{
	}

	public void OnPlayableCreate(Playable playable)
	{
	}

	public void OnPlayableDestroy(Playable playable)
	{
	}

	public void OnBehaviourPlay(Playable playable, FrameData info)
	{
		if (Application.isPlaying && (bool)Owner)
		{
			BarkPlayer.Bark(Owner.EntityData, SharedText.String, Duration);
		}
	}

	public void OnBehaviourPause(Playable playable, FrameData info)
	{
	}

	public void PrepareFrame(Playable playable, FrameData info)
	{
	}

	public void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
	}
}
