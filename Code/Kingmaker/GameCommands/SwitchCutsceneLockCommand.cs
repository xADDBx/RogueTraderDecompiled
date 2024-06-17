namespace Kingmaker.GameCommands;

public class SwitchCutsceneLockCommand : GameCommand
{
	public readonly bool Lock;

	public SwitchCutsceneLockCommand(bool @lock)
	{
		Lock = @lock;
	}

	protected override void ExecuteInternal()
	{
		((IGameDoSwitchCutsceneLock)Game.Instance).DoSwitchCutsceneLock(Lock);
	}
}
