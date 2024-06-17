using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Menu;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame;

public class NewGameVM : NewGamePhaseBaseVm
{
	public readonly SelectionGroupRadioVM<NewGameMenuEntityVM> MenuSelectionGroup;

	public readonly ReactiveProperty<NewGameMenuEntityVM> SelectedMenuEntity = new ReactiveProperty<NewGameMenuEntityVM>();

	private readonly List<NewGameMenuEntityVM> m_MenuEntitiesList = new List<NewGameMenuEntityVM>();

	public readonly ReactiveCommand ChangeTab = new ReactiveCommand();

	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty();

	public NewGamePhaseStoryVM StoryVM { get; }

	public NewGamePhaseDifficultyVM DifficultyVM { get; }

	public NewGameVM(Action backStep, Action nextStep, BoolReactiveProperty isActive)
		: base(backStep, nextStep)
	{
		AddDisposable(StoryVM = new NewGamePhaseStoryVM(OnBack, delegate
		{
			SelectedMenuEntity.Value = m_MenuEntitiesList.Find((NewGameMenuEntityVM e) => e.NewGamePhaseVM == DifficultyVM);
		}));
		AddDisposable(DifficultyVM = new NewGamePhaseDifficultyVM(delegate
		{
			SettingsController.Instance.ConfirmAllTempValues();
			SettingsController.Instance.SaveAll();
			SelectedMenuEntity.Value = m_MenuEntitiesList.Find((NewGameMenuEntityVM e) => e.NewGamePhaseVM == StoryVM);
		}, DifficultyNexStep));
		CreateMenuEntity(UIStrings.Instance.NewGameWin.ScenarioMenuLabel, StoryVM, OnStoryMenuSelect);
		CreateMenuEntity(UIStrings.Instance.NewGameWin.DifficultyMenuLabel, DifficultyVM, OnDifficultyMenuSelect);
		MenuSelectionGroup = new SelectionGroupRadioVM<NewGameMenuEntityVM>(m_MenuEntitiesList, SelectedMenuEntity);
		SelectedMenuEntity.Value = m_MenuEntitiesList.First();
		AddDisposable(MenuSelectionGroup);
		AddDisposable(StoryVM.IsNextButtonAvailable.Subscribe(OnNextMenuEntityAvailable));
		IsActive = isActive;
	}

	private void DifficultyNexStep()
	{
		if (SettingsRoot.Difficulty.GameDifficulty.GetTempValue() >= GameDifficultyOption.Core)
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
			{
				h.HandleOpen(UIStrings.Instance.NewGameWin.AreYouSureChooseVeryHardDifficulty, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton buttonPressed)
				{
					if (buttonPressed == DialogMessageBoxBase.BoxButton.Yes)
					{
						OnNext();
					}
				});
			});
		}
		else
		{
			OnNext();
		}
	}

	private void OnNextMenuEntityAvailable(bool value)
	{
		for (int i = MenuSelectionGroup.EntitiesCollection.IndexOf(MenuSelectionGroup.SelectedEntity.Value) + 1; i < MenuSelectionGroup.EntitiesCollection.Count; i++)
		{
			MenuSelectionGroup.EntitiesCollection[i].SetAvailable(value);
		}
	}

	private void CreateMenuEntity(LocalizedString localizedString, NewGamePhaseBaseVm newGamePhaseVM, Action callback)
	{
		NewGameMenuEntityVM newGameMenuEntityVM = new NewGameMenuEntityVM(localizedString, newGamePhaseVM, callback);
		AddDisposable(newGameMenuEntityVM);
		m_MenuEntitiesList.Add(newGameMenuEntityVM);
	}

	private void OnStoryMenuSelect()
	{
		DifficultyVM.SetEnabled(value: false);
		StoryVM.SetEnabled(value: true);
		ChangeTab.Execute();
	}

	private void OnDifficultyMenuSelect()
	{
		DifficultyVM.SetEnabled(value: true);
		StoryVM.SetEnabled(value: false);
		ChangeTab.Execute();
	}

	public void OnStart()
	{
		StoryVM.SetEnabled(value: true);
	}

	public void OnButtonBack()
	{
		MenuSelectionGroup.SelectedEntity.Value.OnBack();
	}

	public void OnButtonNext()
	{
		MenuSelectionGroup.SelectedEntity.Value.OnNext();
	}

	public void OnClose()
	{
		OnBack();
	}
}
