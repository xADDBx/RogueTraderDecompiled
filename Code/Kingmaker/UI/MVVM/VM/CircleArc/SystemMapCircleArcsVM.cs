using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CircleArc;

public class SystemMapCircleArcsVM : BaseCircleArcsVM, IStarSystemShipMovementHandler, ISubscriber
{
	protected override void OnGameModeStartImpl(GameModeType gameMode)
	{
		IsCorrectGameMode.Value = gameMode == GameModeType.StarSystem || (gameMode == GameModeType.Dialog && PreviousMode == GameModeType.StarSystem);
	}

	protected override void OnGameModeStopImpl(GameModeType gameMode)
	{
		IsCorrectGameMode.Value = Game.Instance.CurrentMode == GameModeType.StarSystem || (Game.Instance.CurrentMode == GameModeType.Dialog && PreviousMode == GameModeType.StarSystem);
	}

	public void HandleStarSystemShipMovementStarted()
	{
		ShouldMoveArcs.Value = true;
	}

	public void HandleStarSystemShipMovementEnded()
	{
		ShouldMoveArcs.Value = false;
	}
}
