using Kingmaker.GameModes;

namespace Kingmaker.GameCommands;

public class StopGameModeCommand : ChangeGameModeCommand
{
	public StopGameModeCommand(GameModeType gameMode)
		: base(ActionType.Stop, gameMode)
	{
	}

	protected override void ExecuteInternal()
	{
		((IGameDoStopMode)Game.Instance).DoStopMode(GameMode);
	}
}
