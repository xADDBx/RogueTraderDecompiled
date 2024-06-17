using System;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.GameOver;

public class GameOverVM : CommonStaticComponentVM
{
	public readonly ReactiveProperty<string> Reason = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> CanQuickLoad = new ReactiveProperty<bool>(initialValue: false);

	public GameOverVM()
	{
		Reason.Value = GetReasonString();
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			CanQuickLoad.Value = Game.Instance.SaveManager.GetLatestSave() != null;
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	private string GetReasonString()
	{
		string result = string.Empty;
		switch (Game.Instance.Player.GameOverReason)
		{
		case Player.GameOverReasonType.PartyIsDefeated:
			result = UIGameOverScreen.Instance.PartyIsDefeatedLabel;
			break;
		case Player.GameOverReasonType.EssentialUnitIsDead:
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.State.AllBaseUnits.FirstOrDefault((BaseUnitEntity c) => c.LifeState.IsFinallyDead && c.IsEssentialForGame);
			result = ((baseUnitEntity != null) ? string.Format((baseUnitEntity.Gender == Gender.Female) ? UIGameOverScreen.Instance.FemaleDeadLabel : UIGameOverScreen.Instance.MaleDeadLabel, baseUnitEntity.CharacterName) : ((string)UIGameOverScreen.Instance.PartyIsDefeatedLabel));
			break;
		}
		case Player.GameOverReasonType.KingdomIsDestroyed:
			result = UIGameOverScreen.Instance.PartyIsDefeatedLabel;
			break;
		case Player.GameOverReasonType.QuestFailed:
			result = UIGameOverScreen.Instance.QuestIsFailedLabel;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Player.GameOverReasonType.Won:
			break;
		}
		return result;
	}

	public void OnButtonLoadGame()
	{
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Load, singleMode: true);
		});
	}

	public void OnQuickLoad()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			Game.Instance.LoadGame(Game.Instance.SaveManager.GetLatestSave());
		}));
	}

	public void OnButtonMainMenu()
	{
		Game.Instance.ResetToMainMenu();
	}
}
