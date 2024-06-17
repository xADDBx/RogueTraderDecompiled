using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.BugReport;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.Code.UI.MVVM.VM.EscMenu;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Replay;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.VM.Credits;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using Photon.Realtime;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Common;

public class CommonVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IDialogMessageBoxUIHandler, ICounterWindowUIHandler, IContextMenuHandler, ISaveLoadUIHandler, IUILoadService, ISettingsUIHandler, IAreaHandler, IBugReportUIHandler, IFullScreenUIHandler, INetLobbyRequest, INetRolesRequest, INetInviteHandler, INetLobbyPlayersHandler, IEndGameTitlesUIHandler, IAdditiveAreaSwitchHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, IVendorUIHandler, ISubscriber<IMechanicEntity>, IMultiEntranceHandler
{
	public readonly UIVisibilityVM UIVisibilityVM;

	public readonly EscMenuContextVM EscMenuContextVM;

	public readonly TooltipContextVM TooltipContextVM;

	public readonly QuestNotificatorVM QuestNotificatorVM;

	public readonly GamepadConnectDisconnectVM GamepadConnectDisconnectVM;

	public readonly UnitStatesHolderVM UnitStatesHolderVM;

	public readonly StarSystemObjectStateVM StarSystemObjectStateVM;

	public readonly WarningsTextVM WarningsTextVM;

	public readonly FadeVM FadeVM;

	public readonly TutorialVM TutorialVM;

	public readonly ReactiveProperty<MessageBoxVM> MessageBoxVM = new ReactiveProperty<MessageBoxVM>();

	public readonly ReactiveProperty<CounterWindowVM> CounterWindowVM = new ReactiveProperty<CounterWindowVM>();

	public readonly ReactiveProperty<ContextMenuVM> ContextMenuVM = new ReactiveProperty<ContextMenuVM>();

	public readonly ReactiveProperty<SaveLoadVM> SaveLoadVM = new ReactiveProperty<SaveLoadVM>();

	public readonly ReactiveProperty<SettingsVM> SettingsVM = new ReactiveProperty<SettingsVM>();

	public readonly ReactiveProperty<BugReportVM> BugReportVM = new ReactiveProperty<BugReportVM>();

	public readonly ReactiveProperty<NetLobbyVM> NetLobbyVM = new ReactiveProperty<NetLobbyVM>(null);

	public readonly ReactiveProperty<NetRolesVM> NetRolesVM = new ReactiveProperty<NetRolesVM>(null);

	public readonly ReactiveProperty<TitlesVM> TitlesVM = new ReactiveProperty<TitlesVM>();

	private readonly Queue<MessageBoxVM> m_MessageQueue = new Queue<MessageBoxVM>();

	private bool m_EnterGameStarted;

	public TooltipsDataCache TooltipsDataCache => TooltipContextVM?.TooltipsDataCache;

	public CommonVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
		AddDisposable(EscMenuContextVM = new EscMenuContextVM());
		AddDisposable(TooltipContextVM = new TooltipContextVM());
		AddDisposable(QuestNotificatorVM = new QuestNotificatorVM());
		AddDisposable(GamepadConnectDisconnectVM = new GamepadConnectDisconnectVM());
		AddDisposable(UnitStatesHolderVM = new UnitStatesHolderVM());
		AddDisposable(StarSystemObjectStateVM = new StarSystemObjectStateVM());
		AddDisposable(WarningsTextVM = new WarningsTextVM());
		AddDisposable(FadeVM = new FadeVM());
		AddDisposable(TutorialVM = new TutorialVM());
		AddDisposable(Game.Instance.Keyboard.Bind("SwitchUIVisibility", UIVisibilityState.SwitchVisibility));
	}

	protected override void DisposeImplementation()
	{
		DisposeMessageQueue();
		DisposeMessageBox();
		DisposeCounterWindow();
		DisposeContextMenu();
		DisposeSaveLoad();
		DisposeSettings();
		DisposeNetLobby();
		DisposeNetRoles();
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
		BugReportVM disposable = (BugReportVM.Value = new BugReportVM());
		AddDisposable(disposable);
		if (!Kingmaker.Replay.Replay.IsActive && !NetworkingManager.IsActive)
		{
			Game.Instance.StartMode(GameModeType.BugReport);
		}
	}

	public void HandleBugReportHide()
	{
		DisposeAndRemove(BugReportVM);
		Game.Instance.StopMode(GameModeType.BugReport);
	}

	public void HandleUIElementFeature(string featureName)
	{
	}

	public void CloseTutorialOnLoad()
	{
		if (Game.Instance.Player.Tutorial.HasShownData)
		{
			EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
			{
				h.HideTutorial(Game.Instance.Player.Tutorial.ShowingData);
			});
		}
	}

	public void HandleOpen(string messageText, DialogMessageBoxBase.BoxType boxType = DialogMessageBoxBase.BoxType.Message, Action<DialogMessageBoxBase.BoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue)
	{
		MessageBoxVM messageBoxVM = new MessageBoxVM(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, onTextResult, inputText, inputPlaceholder, waitTime, DisposeMessageBox);
		if (MessageBoxVM.Value == null)
		{
			MessageBoxVM.Value = messageBoxVM;
		}
		else
		{
			m_MessageQueue.Enqueue(messageBoxVM);
		}
	}

	public void HandleClose()
	{
		DisposeMessageBox();
	}

	private void DisposeMessageBox()
	{
		DisposeAndRemove(MessageBoxVM);
		if (m_MessageQueue.Count > 0)
		{
			MessageBoxVM value = m_MessageQueue.Dequeue();
			MessageBoxVM.Value = value;
		}
	}

	private void DisposeMessageQueue()
	{
		while (m_MessageQueue.Count > 0)
		{
			m_MessageQueue.Dequeue().Dispose();
		}
	}

	public void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command)
	{
		DisposeCounterWindow();
		CounterWindowVM disposable = (CounterWindowVM.Value = new CounterWindowVM(type, item, command, DisposeCounterWindow));
		AddDisposable(disposable);
	}

	private void DisposeCounterWindow()
	{
		CounterWindowVM.Value?.Dispose();
		CounterWindowVM.Value = null;
	}

	public void HandleContextMenuRequest(ContextMenuCollection collection)
	{
		DisposeContextMenu();
		if (collection != null)
		{
			ContextMenuVM disposable = (ContextMenuVM.Value = new ContextMenuVM(collection));
			AddDisposable(disposable);
		}
	}

	private void DisposeContextMenu()
	{
		ContextMenuVM.Value?.Dispose();
		ContextMenuVM.Value = null;
	}

	public void HandleOpenSaveLoad(SaveLoadMode mode, bool singleMode)
	{
		IUILoadService iUILoadService2;
		if (!Game.Instance.RootUiContext.IsMainMenu)
		{
			IUILoadService iUILoadService = new CommonUILoadService(DisposeSaveLoad);
			iUILoadService2 = iUILoadService;
		}
		else
		{
			IUILoadService iUILoadService = this;
			iUILoadService2 = iUILoadService;
		}
		IUILoadService loadService = iUILoadService2;
		SaveLoadVM disposable = (SaveLoadVM.Value = new SaveLoadVM(mode, singleMode, DisposeSaveLoad, loadService));
		AddDisposable(disposable);
	}

	public void Load(SaveInfo saveInfo)
	{
		Load(saveInfo, null);
	}

	public void Load(SaveInfo saveInfo, [CanBeNull] Action callback)
	{
		DisposeSaveLoad();
		EnterGame(delegate
		{
			Game.Instance.LoadGameFromMainMenu(saveInfo, callback);
		}, saveInfo);
	}

	public void DisposeSaveLoad()
	{
		SaveLoadVM.Value?.Dispose();
		SaveLoadVM.Value = null;
	}

	private void EnterGame(Action action, SaveInfo saveToLoad)
	{
		if (m_EnterGameStarted)
		{
			UberDebug.LogError("Double game start detected!");
		}
		else
		{
			LoadingProcess.Instance.StartLoadingProcess(EnterGameCoroutine(action));
		}
	}

	private IEnumerator EnterGameCoroutine(Action action)
	{
		m_EnterGameStarted = true;
		yield return null;
		SceneLoader.LoadObligatoryScenes();
		yield return null;
		action?.Invoke();
	}

	public void HandleOpenSettings(bool isMainMenu = false)
	{
		SettingsVM disposable = (SettingsVM.Value = new SettingsVM(DisposeSettings, isMainMenu));
		AddDisposable(disposable);
	}

	private void DisposeSettings()
	{
		SettingsVM.Value?.Dispose();
		SettingsVM.Value = null;
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		if (NetLobbyVM.Value == null)
		{
			NetLobbyVM.Value = new NetLobbyVM(delegate
			{
				DisposeNetLobby(isMainMenu);
			});
			if (isMainMenu)
			{
				SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.CoopLobby);
			}
		}
	}

	public void HandleNetLobbyClose()
	{
	}

	private void DisposeNetLobby(bool isMainMenu = false)
	{
		NetLobbyVM.Value?.Dispose();
		NetLobbyVM.Value = null;
		if (isMainMenu)
		{
			SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.MainMenu);
		}
	}

	public void HandleNetRolesRequest()
	{
		NetRolesVM.Value = new NetRolesVM(DisposeNetRoles);
	}

	private void DisposeNetRoles()
	{
		NetRolesVM.Value?.Dispose();
		NetRolesVM.Value = null;
	}

	public void HandleShowEndGameTitles(bool returnToMainMenu = true)
	{
		TitlesVM disposable = (TitlesVM.Value = new TitlesVM(delegate
		{
			TitlesVM.Value?.Dispose();
			TitlesVM.Value = null;
			if (returnToMainMenu)
			{
				Game.Instance.ResetToMainMenu();
			}
		}));
		AddDisposable(disposable);
	}

	public void HandleInvite(Action<bool> callback)
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.InviteLobbyMessageBox, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			callback?.Invoke(button == DialogMessageBoxBase.BoxButton.Yes);
		});
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		CoroutineRunner.Start(SendWarningCoroutine(player));
		static float CurrentTimeSeconds()
		{
			return Time.realtimeSinceStartup;
		}
		static IEnumerator SendWarningCoroutine(Photon.Realtime.Player player)
		{
			float timeoutSeconds = CurrentTimeSeconds() + 2f;
			while (string.IsNullOrEmpty(player.NickName))
			{
				yield return null;
				if (timeoutSeconds < CurrentTimeSeconds())
				{
					yield break;
				}
			}
			UIUtility.SendWarning(string.Format((PhotonManager.NetGame.CurrentState != NetGame.State.Playing) ? UIStrings.Instance.NetLobbyTexts.NewPlayerJoinToLobby : UIStrings.Instance.NetLobbyTexts.NewPlayerJoinToActiveLobby, player.NickName));
		}
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		if (PhotonManager.NetGame.CurrentState == NetGame.State.Playing)
		{
			UIUtility.SendWarning(string.Format(UIStrings.Instance.NetLobbyTexts.PlayerLeftRoomWarning, player.NickName));
		}
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.LastPlayerLeftLobbyMessageBox, DialogMessageBoxBase.BoxType.Message, delegate
		{
		});
	}

	public void HandleRoomOwnerChanged()
	{
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		Game instance = Game.Instance;
		if (fullScreenUIType != FullScreenUIType.LocalMap && NetLobbyVM.Value == null && NetRolesVM.Value == null && SaveLoadVM.Value == null && SettingsVM.Value == null && BugReportVM.Value == null)
		{
			instance.RequestPauseUi(state);
		}
	}

	private void ForceDisposeAllFullscreen()
	{
		TooltipContextVM.DisposeAll();
		TooltipsDataCache?.Clear();
		DisposeMessageBox();
		DisposeSettings();
		HandleBugReportHide();
		DisposeNetLobby();
		DisposeSaveLoad();
		DisposeContextMenu();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportHide();
		});
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap || gameMode == GameModeType.StarSystem || gameMode == GameModeType.Dialog)
		{
			ForceDisposeAllFullscreen();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		ForceDisposeAllFullscreen();
		UnitStatesHolderVM.Clear();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		ForceDisposeAllFullscreen();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			ForceDisposeAllFullscreen();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleTradeStarted()
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		ForceDisposeAllFullscreen();
	}
}
