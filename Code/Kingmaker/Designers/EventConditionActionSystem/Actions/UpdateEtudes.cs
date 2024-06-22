using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5afa2802918b3874bb6e0d611d6489cd")]
public class UpdateEtudes : GameAction
{
	public override string GetDescription()
	{
		return $"Обновляет стейт этюдов. Перепроверяет кондишены и подобное";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.EtudesSystem.UpdateEtudes();
	}

	public override string GetCaption()
	{
		return $"Update etudes";
	}
}
