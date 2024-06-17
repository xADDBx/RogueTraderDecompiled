using System;

namespace Kingmaker.AreaLogic.Cutscenes;

public class FailedToStopCutsceneCommandException : CutsceneCommandException
{
	public FailedToStopCutsceneCommandException(CutscenePlayerData player, CommandBase command, Exception e)
		: base(player, command, CutsceneCommandException.GetMessage(player, command, e, "Stop"), e)
	{
	}
}
