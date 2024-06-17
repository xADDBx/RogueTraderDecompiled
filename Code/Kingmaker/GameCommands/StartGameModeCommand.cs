using Kingmaker.GameModes;

namespace Kingmaker.GameCommands;

public class StartGameModeCommand : ChangeGameModeCommand
{
	public StartGameModeCommand(GameModeType gameMode)
		: base(ActionType.Start, gameMode)
	{
	}

	protected override void ExecuteInternal()
	{
		((IGameDoStartMode)Game.Instance).DoStartMode(GameMode);
	}
}
