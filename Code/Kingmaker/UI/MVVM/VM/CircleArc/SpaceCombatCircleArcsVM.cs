using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CircleArc;

public class SpaceCombatCircleArcsVM : BaseCircleArcsVM, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	protected override void OnGameModeStartImpl(GameModeType gameMode)
	{
		IsCorrectGameMode.Value = gameMode == GameModeType.SpaceCombat;
	}

	protected override void OnGameModeStopImpl(GameModeType gameMode)
	{
		IsCorrectGameMode.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateTurn(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateTurn(isTurnBased: true);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdateTurn(isTurnBased);
	}

	private void UpdateTurn(bool isTurnBased)
	{
		TurnController turnController = Game.Instance.TurnController;
		bool value = isTurnBased && !turnController.IsPlayerTurn;
		ShouldMoveArcs.Value = value;
	}
}
