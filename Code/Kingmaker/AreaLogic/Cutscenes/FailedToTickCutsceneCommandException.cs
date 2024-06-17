using System;

namespace Kingmaker.AreaLogic.Cutscenes;

public class FailedToTickCutsceneCommandException : CutsceneCommandException
{
	public FailedToTickCutsceneCommandException(CutscenePlayerData player, CommandBase command, Exception e)
		: base(player, command, CutsceneCommandException.GetMessage(player, command, e, "Tick"), e)
	{
	}
}
