using System;

namespace Kingmaker.AreaLogic.Cutscenes;

public class FailedToRunCutsceneCommandException : CutsceneCommandException
{
	public FailedToRunCutsceneCommandException(CutscenePlayerData player, CommandBase command, Exception e)
		: base(player, command, CutsceneCommandException.GetMessage(player, command, e, "Run"), e)
	{
	}
}
