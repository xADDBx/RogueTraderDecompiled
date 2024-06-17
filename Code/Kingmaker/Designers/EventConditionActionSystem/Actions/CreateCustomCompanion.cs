using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c44f8f7b59c1b9145af6c8d5e4481a8d")]
public class CreateCustomCompanion : GameAction
{
	[SerializeReference]
	public LocatorEvaluator Locator;

	public bool ForFree;

	public bool MatchPlayerXpExactly;

	public CharGenConfig.CharGenCompanionType CompanionType;

	public ActionList OnCreate = new ActionList();

	public override string GetDescription()
	{
		return $"Вызывает окно создания компаньона, если достаточно профит фактора. \nКомпаньона можно сделать бесплатным галкой ForFree ({ForFree})\n Можно подтянуть опыт компаньона под опыт персонажа галкой MatchPlayerXpExactly ({MatchPlayerXpExactly})\n Можно выполнить экшены при создании";
	}

	public override string GetCaption()
	{
		return "Create custom companion";
	}

	public override void RunAction()
	{
		Player player = Game.Instance.Player;
		if (!ForFree && (float)player.GetCustomCompanionCost() > player.ProfitFactor.Total)
		{
			PFLog.Default.Error("Has no enough profit factor for create custom companion");
			return;
		}
		Game.Instance.Player.CreateCustomCompanion(delegate(BaseUnitEntity newCompanion)
		{
			if (!ForFree)
			{
				player.ProfitFactor.AddModifier(-player.GetCustomCompanionCost(), ProfitFactorModifierType.Companion);
			}
			if (Locator != null && Locator.CanEvaluate())
			{
				LocatorEntity value = Locator.GetValue();
				newCompanion.AttachView(newCompanion.CreateView());
				newCompanion.Position = value.Position;
				if (value.View != null)
				{
					newCompanion.SetOrientation(value.View.ViewTransform.rotation.eulerAngles.y);
				}
				SceneEntitiesState crossSceneState = Game.Instance.State.PlayerState.CrossSceneState;
				Game.Instance.EntitySpawner.SpawnEntityImmediately(newCompanion, crossSceneState);
				Game.Instance.Player.AddCompanion(newCompanion, remote: true);
				newCompanion.IsInGame = true;
			}
			OnCreate.Run();
		}, MatchPlayerXpExactly ? new int?(GameHelper.GetPlayerCharacter().Progression.Experience) : null, CompanionType);
	}
}
