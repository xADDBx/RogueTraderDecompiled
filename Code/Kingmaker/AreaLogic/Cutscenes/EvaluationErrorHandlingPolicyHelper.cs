using JetBrains.Annotations;

namespace Kingmaker.AreaLogic.Cutscenes;

public static class EvaluationErrorHandlingPolicyHelper
{
	public static EvaluationErrorHandlingPolicy GetEvaluationErrorHandlingPolicy(CutscenePlayerData player, [CanBeNull] CutscenePlayerTrackData track, [CanBeNull] CommandBase command, out CutsceneElement cutsceneElement)
	{
		if (command != null && command.EvaluationErrorHandlingPolicy != 0)
		{
			cutsceneElement = CutsceneElement.Command;
			return command.EvaluationErrorHandlingPolicy;
		}
		if (track != null && track.Track.EvaluationErrorHandlingPolicy != 0)
		{
			cutsceneElement = CutsceneElement.Track;
			return track.Track.EvaluationErrorHandlingPolicy;
		}
		if (track != null && track.StartGate.Gate.EvaluationErrorHandlingPolicy != 0)
		{
			cutsceneElement = CutsceneElement.Gate;
			return track.StartGate.Gate.EvaluationErrorHandlingPolicy;
		}
		cutsceneElement = CutsceneElement.Cutscene;
		return player.Cutscene.EvaluationErrorHandlingPolicy;
	}
}
