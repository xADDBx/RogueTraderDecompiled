namespace Kingmaker.Controllers;

public interface IGameTimeController
{
	GameTimeState TimeState { get; }

	void SetState(GameTimeState state);
}
