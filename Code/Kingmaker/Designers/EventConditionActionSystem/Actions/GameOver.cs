using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c874c271d8d54eb692490a97e6f0a19d")]
public class GameOver : GameAction
{
	public Player.GameOverReasonType Reason;

	public override string GetDescription()
	{
		return $"Завершает игру по указанной причине {Reason}";
	}

	public override string GetCaption()
	{
		return "Game over: " + Reason;
	}

	public override void RunAction()
	{
		Game.Instance.Player.GameOver(Reason);
	}
}
