using JetBrains.Annotations;
using Kingmaker.DialogSystem.Blueprints;

namespace Kingmaker.Controllers.Dialog;

public class CueSequence
{
	public readonly BlueprintCueSequence Blueprint;

	private int m_NextCueIndex;

	public CueSequence(BlueprintCueSequence blueprint)
	{
		Blueprint = blueprint;
		m_NextCueIndex = 0;
	}

	[CanBeNull]
	public BlueprintCueBase PollNextCue()
	{
		while (m_NextCueIndex < Blueprint.Cues.Count)
		{
			BlueprintCueBase blueprintCueBase = Blueprint.Cues[m_NextCueIndex].Get();
			if (blueprintCueBase != null && blueprintCueBase.CanShow())
			{
				break;
			}
			m_NextCueIndex++;
		}
		while (m_NextCueIndex < Blueprint.Cues.Count)
		{
			int index = m_NextCueIndex++;
			BlueprintCueBase blueprintCueBase2 = Blueprint.Cues[index].Get();
			if (blueprintCueBase2 != null)
			{
				DialogDebug.Add(blueprintCueBase2, "next sequence cue");
				return blueprintCueBase2;
			}
		}
		return null;
	}
}
