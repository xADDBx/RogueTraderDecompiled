using System;
using Kingmaker.GameCommands;
using UnityEngine.Video;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

public class InterchapterData
{
	public VideoClip VideoClip;

	public string SoundStartEvent;

	public string SoundStopEvent;

	public bool Finished;

	public VideoState? State { get; private set; }

	public TimeSpan StateStartTime { get; private set; } = TimeSpan.Zero;


	public void Finish()
	{
		Game.Instance.GameCommandQueue.SkipCutscene();
	}

	public void SetState(VideoState state)
	{
		State = state;
		StateStartTime = Game.Instance.Player.RealTime;
	}
}
