using System;
using System.Collections;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.NewGame;
using Kingmaker.Code.UI.MVVM.VM.TermOfUse;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.MainMenu;
using Kingmaker.Utility;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.MainMenu;

public class MainMenuVM : VMBase, IUIMainMenu
{
	public readonly MainMenuSideBarVM MainMenuSideBarVM;

	private bool m_IsChargenMusicTheme;

	private ChargenUnit m_ChargenUnit;

	public readonly CharGenContextVM CharGenContextVM;

	public readonly ReactiveProperty<NewGameVM> NewGameVM = new ReactiveProperty<NewGameVM>();

	public readonly ReactiveProperty<FeedbackPopupVM> FeedbackPopupVM = new ReactiveProperty<FeedbackPopupVM>();

	public readonly ReactiveProperty<TermsOfUseVM> TermsOfUseVM = new ReactiveProperty<TermsOfUseVM>();

	public readonly ReactiveProperty<CreditsVM> CreditsVM = new ReactiveProperty<CreditsVM>();

	public readonly ReactiveProperty<FirstLaunchSettingsVM> FirstLaunchSettings = new ReactiveProperty<FirstLaunchSettingsVM>();

	private readonly ReactiveCommand m_OpenCharGenCommand = new ReactiveCommand();

	private readonly BoolReactiveProperty m_NewGameIsActive = new BoolReactiveProperty();

	private readonly ReactiveCommand m_OpenCreditsCommand = new ReactiveCommand();

	private readonly ReactiveCommand m_PlayFirstLaunchFXCommand = new ReactiveCommand();

	private bool m_EnterGameStarted;

	public IObservable<Unit> OpenCharGenCommand => m_OpenCharGenCommand;

	public IObservable<Unit> OpenCreditsCommand => m_OpenCreditsCommand;

	public IObservable<Unit> PlayFirstLaunchFXCommand => m_PlayFirstLaunchFXCommand;

	public MainMenuVM()
	{
		MainMenuUI.Instance = this;
		SoundBanksManager.LoadVoiceBanks();
		AddDisposable(MainMenuSideBarVM = new MainMenuSideBarVM(this));
		AddDisposable(new MainMenuChargenUnits());
		AddDisposable(CharGenContextVM = new CharGenContextVM());
		if (!FirstLaunchSettingsVM.HasShown)
		{
			AddDisposable(FirstLaunchSettings.Value = new FirstLaunchSettingsVM(OnCloseFirstLaunchSettingsVM));
		}
	}

	protected override void DisposeImplementation()
	{
		DisposeNewGame();
		SoundBanksManager.UnloadVoiceBanks();
		MainMenuUI.Instance = null;
		SaveScreenshotManager.Instance.Cleanup();
	}

	private void OnCloseFirstLaunchSettingsVM()
	{
		FirstLaunchSettings.Value.Dispose();
		FirstLaunchSettings.Value = null;
		ShowLicense(delegate
		{
			PhotonManager.Invite.CheckAvailableInvite();
		});
	}

	public void ShowNewGameSetup()
	{
		m_NewGameIsActive.Value = true;
		ReactiveProperty<NewGameVM> newGameVM = NewGameVM;
		if (newGameVM.Value == null)
		{
			NewGameVM newGameVM3 = (newGameVM.Value = new NewGameVM(BackStep, NextStep, m_NewGameIsActive));
		}
		UpdateSoundState();
		void BackStep()
		{
			Game.Instance.Player.RemoveAllAdditionalContentDlc();
			DisposeNewGame();
			UpdateSoundState();
		}
		void NextStep()
		{
			m_NewGameIsActive.Value = false;
			if (m_ChargenUnit == null)
			{
				BlueprintUnit defaultPlayerCharacter = BlueprintRoot.Instance.DefaultPlayerCharacter;
				m_ChargenUnit = new ChargenUnit(defaultPlayerCharacter);
			}
			else if (m_ChargenUnit.Used)
			{
				m_ChargenUnit.RecreateUnit();
			}
			m_ChargenUnit.Used = true;
			CharGenConfig.Create(m_ChargenUnit.Unit, CharGenConfig.CharGenMode.NewGame).SetOnComplete(delegate(BaseUnitEntity newUnit)
			{
				Game.NewGameUnit = newUnit;
				Game.Instance.Player.SetMainCharacter(newUnit);
			}).SetEnterNewGameAction(delegate
			{
				EnterGame(Game.Instance.LoadNewGame);
			})
				.SetOnCloseSoundAction(UpdateSoundState)
				.SetOnShowNewGameAction(delegate
				{
					m_NewGameIsActive.Value = true;
				})
				.OpenUI();
		}
	}

	private void DisposeNewGame()
	{
		m_NewGameIsActive.Value = false;
		NewGameVM.Value?.Dispose();
		NewGameVM.Value = null;
	}

	private void UpdateSoundState()
	{
		bool flag = NewGameVM.Value != null || CharGenContextVM?.CharGenVM.Value != null;
		if (m_IsChargenMusicTheme != flag)
		{
			m_IsChargenMusicTheme = flag;
			SoundState.Instance.OnChargenChange(m_IsChargenMusicTheme);
		}
	}

	public void LoadLastGame()
	{
		EnterGame(LoadLastSave);
	}

	public void ShowNetLobby()
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest(isMainMenu: true);
		});
	}

	public void ShowDlcManager()
	{
		EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
		{
			h.HandleOpenDlcManager();
		});
	}

	public void OpenSettings()
	{
		EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
		{
			h.HandleOpenSettings(isMainMenu: true);
		});
	}

	public void Exit()
	{
		SystemUtil.ApplicationQuit();
	}

	public void ShowLicense()
	{
		ShowLicense(null);
	}

	private void ShowLicense(Action onClose)
	{
		ReactiveProperty<TermsOfUseVM> termsOfUseVM = TermsOfUseVM;
		if (termsOfUseVM.Value == null)
		{
			TermsOfUseVM termsOfUseVM3 = (termsOfUseVM.Value = new TermsOfUseVM(CloseAction));
		}
		void CloseAction()
		{
			TermsOfUseVM.Value?.Dispose();
			TermsOfUseVM.Value = null;
			if (!FirstLaunchSettingsVM.HasShown)
			{
				FirstLaunchSettingsVM.SetFirstLaunchPrefs();
				SoundState.Instance.MusicStateHandler.StartMusicPlayEvent();
				PlayFirstLaunchFX();
				onClose?.Invoke();
			}
		}
	}

	private void PlayFirstLaunchFX()
	{
		m_PlayFirstLaunchFXCommand.Execute();
	}

	public void ShowCredits()
	{
		CreditsVM.Value = new CreditsVM(CloseAction);
		SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.Credits);
		void CloseAction()
		{
			CreditsVM.Value?.Dispose();
			CreditsVM.Value = null;
			SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.MainMenu);
		}
	}

	public void ShowFeedback()
	{
		ReactiveProperty<FeedbackPopupVM> feedbackPopupVM = FeedbackPopupVM;
		if (feedbackPopupVM.Value == null)
		{
			FeedbackPopupVM feedbackPopupVM3 = (feedbackPopupVM.Value = new FeedbackPopupVM(CloseAction));
		}
		void CloseAction()
		{
			FeedbackPopupVM.Value?.Dispose();
			FeedbackPopupVM.Value = null;
		}
	}

	public void EnterGame(Action action)
	{
		if (m_EnterGameStarted)
		{
			PFLog.UI.Error("Double game start detected!");
		}
		else
		{
			CoroutineRunner.Start(EnterGameCoroutine(action));
		}
	}

	private IEnumerator EnterGameCoroutine(Action action)
	{
		m_EnterGameStarted = true;
		Game.Instance.RootUiContext.CommonVM.CloseTutorialOnLoad();
		Game.Instance.RootUiContext.LoadingScreenRootVM.ShowLoadingScreen();
		yield return null;
		Runner.ShouldStartManually = true;
		yield return SceneLoader.LoadObligatoryScenesAsync();
		action();
		yield return null;
		Runner.StartManually();
		m_EnterGameStarted = false;
		Runner.ShouldStartManually = false;
	}

	private void LoadLastSave()
	{
		Game.Instance.SaveManager.UpdateSaveListIfNeeded();
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			SaveInfo anyLatestSave = Game.Instance.SaveManager.GetAnyLatestSave();
			if (anyLatestSave != null)
			{
				Game.Instance.LoadGameFromMainMenu(anyLatestSave);
			}
			else
			{
				Game.Instance.LoadNewGame();
			}
		}));
	}

	public void SetNewGamePreset(BlueprintAreaPreset newGamePreset)
	{
		Game.NewGamePreset = newGamePreset;
		PFLog.System.Log($"Selected new game preset: {newGamePreset}");
	}

	public void OnEnableGameStatistic(DialogMessageBoxBase.BoxButton buttonType)
	{
		SettingsRoot.Game.Main.SendSaves.SetValueAndConfirm(buttonType == DialogMessageBoxBase.BoxButton.Yes);
		SettingsRoot.Game.Main.SendGameStatistic.SetValueAndConfirm(buttonType == DialogMessageBoxBase.BoxButton.Yes);
		SettingsController.Instance.SaveAll();
	}

	public void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		switch (linkInfo.GetLinkID())
		{
		case "pp":
			Application.OpenURL("https://owlcatgames.com/privacy");
			break;
		case "upp":
			Application.OpenURL("https://unity3d.com/legal/privacy-policy");
			break;
		}
	}
}
