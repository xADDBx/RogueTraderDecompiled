using Kingmaker.GameModes;

namespace Kingmaker.GameCommands;

public abstract class ChangeGameModeCommand : GameCommand
{
	public enum ActionType
	{
		Start,
		Stop
	}

	public readonly ActionType Action;

	public readonly GameModeType GameMode;

	protected ChangeGameModeCommand(ActionType action, GameModeType gameMode)
	{
		Action = action;
		GameMode = gameMode;
	}
}
