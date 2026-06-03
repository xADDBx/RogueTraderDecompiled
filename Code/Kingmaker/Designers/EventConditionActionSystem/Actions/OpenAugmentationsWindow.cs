using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0b3239efbc8041528d5f4278075d0706")]
public class OpenAugmentationsWindow : GameAction
{
	public ActionList OnClose;

	[Tooltip("Должны ли аугментации быть доступными к редактированию после открытия окна")]
	public bool AllowToEdit = true;

	[Tooltip("Должно ли окно аугментаций открываться для всех игроков в коопе или только для того, кто с ним провзаимодействовал")]
	public bool OpenForEveryoneInCoop = true;

	public override string GetCaption()
	{
		return "Open UI Augmentations Window";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = ContextData<InteractingUnitData>.Current?.Unit;
		if (OpenForEveryoneInCoop || baseUnitEntity == null || baseUnitEntity.IsMyNetRole())
		{
			if (AllowToEdit && Game.Instance.Player.PartyAugmentManager.CurrentAvailableTier != 0)
			{
				Game.Instance.LoadedAreaState.Settings.AugmentsViewOnly.ReleaseAll();
			}
			UIUtilityWindows.OpenAugmentationsWindow(delegate
			{
				OnClose?.Run();
			});
		}
	}
}
