using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Kingmaker.UI.MVVM.View.NetLobby.Console.DlcList;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbyLobbyPartConsoleView : NetLobbyLobbyPartBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private List<NetLobbyPlayerConsoleView> m_PlayerList;

	[SerializeField]
	private ConsoleHint m_ShowHideLobbyCodeHint;

	[SerializeField]
	private ConsoleHint m_CopyLobbyCodeHint;

	[SerializeField]
	private OwlcatButton m_SavePartFocusButton;

	[SerializeField]
	private NetLobbyDlcListConsoleView m_DlcListConsoleView;

	private IConsoleHint m_ContinueHint;

	private readonly BoolReactiveProperty m_SaveSlotIsFocused = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_EpicGamesIsFocused = new BoolReactiveProperty();

	private InputLayer m_GamersTagsInputLayer;

	private GridConsoleNavigationBehaviour m_GamersTagsNavigationBehavior;

	private readonly BoolReactiveProperty m_GamersTagsMode = new BoolReactiveProperty();

	private GridConsoleNavigationBehaviour m_MainNavigationBehavior;

	public override void Initialize()
	{
		base.Initialize();
		m_DlcListConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		for (int i = 0; i < m_PlayerList.Count; i++)
		{
			m_PlayerList[i].Bind(base.ViewModel.PlayerVms[i]);
		}
		AddDisposable(base.ViewModel.DlcListVM.Subscribe(m_DlcListConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		m_GamersTagsNavigationBehavior?.UnFocusCurrentEntity();
		m_GamersTagsNavigationBehavior?.Clear();
		m_SaveSlotIsFocused.Value = false;
		base.DestroyViewImplementation();
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowDlcList();
		}, 14, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.DlcList));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.Disconnect("Close_NotPlayingState");
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CharGen.Back));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.Disconnect("Close_PlayingState");
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.DisconnectLobby));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowNetLobbyTutorial();
		}, 19, base.ViewModel.IsAnyTutorialBlocks.And(IsInLobbyPart).And(base.ViewModel.CanConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.HowToPlay));
		AddDisposable(m_CopyLobbyCodeHint.Bind(inputLayer.AddButton(delegate
		{
			CopyLobbyId();
		}, 17, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).ToReactiveProperty())));
		m_CopyLobbyCodeHint.SetLabel(UIStrings.Instance.NetLobbyTexts.CopyLobbyId);
		AddDisposable(m_ShowHideLobbyCodeHint.Bind(inputLayer.AddButton(delegate
		{
			ShowHideLobbyId();
		}, 16, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_ShowHideLobbyCodeHint.SetLabel(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		AddDisposable(ShowCode.Subscribe(delegate(bool state)
		{
			m_ShowHideLobbyCodeHint.SetLabel(state ? UIStrings.Instance.NetLobbyTexts.HideLobbyCode : UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		}));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ResetCurrentSave();
		}, 11, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost).And(m_SaveSlotIsFocused)
			.And(ResetCurrentSaveActive)
			.ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.ResetCurrentSave));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ChooseSave();
		}, 8, IsInLobbyPart.And(LaunchButtonActive.Not()).And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost)
			.And(m_SaveSlotIsFocused)
			.ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ChooseSave();
		}, 10, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost).And(m_SaveSlotIsFocused)
			.And(LaunchButtonActive)
			.ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenEpicGamesLayer();
		}, 8, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(m_EpicGamesIsFocused).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.SignInToEpicGamesStore));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			Launch();
		}, 8, IsInLobbyPart.And(base.ViewModel.NeedReconnect).And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost)
			.And(base.ViewModel.IsSaveAllowed)
			.ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.Reconnect));
		AddDisposable(m_ContinueHint = hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			Launch();
		}, 8, IsInLobbyPart.And(LaunchButtonActive).And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost)
			.And(base.ViewModel.NeedReconnect.Not())
			.ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_ContinueHint.SetLabel(UIStrings.Instance.NetLobbyTexts.Launch);
		AddDisposable(LaunchButtonText.Subscribe(m_ContinueHint.SetLabel));
		m_PlayerList.ForEach(delegate(NetLobbyPlayerConsoleView p)
		{
			p.AddPlayerInput(inputLayer, hintsWidget, ShowGamersTagsMode, base.ViewModel.CanConfirmLaunch);
		});
	}

	private void AddGamersTagsInput(ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(m_GamersTagsNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_GamersTagsInputLayer = m_GamersTagsNavigationBehavior?.GetInputLayer(new InputLayer
		{
			ContextName = "GamersTags"
		});
		m_PlayerList.ForEach(delegate(NetLobbyPlayerConsoleView p)
		{
			p.AddGamerTagInput(m_GamersTagsInputLayer, hintsWidget, CloseGamersTagsMode, base.ViewModel.CanConfirmLaunch);
		});
	}

	private void ShowGamersTagsMode()
	{
		m_GamersTagsMode.Value = true;
		SetGamersTagsNavigation();
		NetLobbyPlayerConsoleView focusedPlayer = m_PlayerList.FirstOrDefault((NetLobbyPlayerConsoleView p) => p.IsFocused.Value);
		GamerTagAndNameBaseView gamerTagAndNameBaseView = m_GamersTagsNavigationBehavior?.Entities.OfType<GamerTagAndNameBaseView>().FirstOrDefault((GamerTagAndNameBaseView e) => e.GetUserId() == focusedPlayer.Or(null)?.GetUserId());
		GamePad.Instance.PushLayer(m_GamersTagsInputLayer);
		m_MainNavigationBehavior.UnFocusCurrentEntity();
		if (gamerTagAndNameBaseView != null)
		{
			m_GamersTagsNavigationBehavior?.FocusOnEntityManual(gamerTagAndNameBaseView);
		}
		else
		{
			m_GamersTagsNavigationBehavior?.FocusOnFirstValidEntity();
		}
	}

	private void CloseGamersTagsMode()
	{
		m_GamersTagsNavigationBehavior?.UnFocusCurrentEntity();
		m_MainNavigationBehavior?.FocusOnCurrentEntity();
		m_GamersTagsMode.Value = false;
		GamePad.Instance.PopLayer(m_GamersTagsInputLayer);
	}

	private void SetGamersTagsNavigation()
	{
		m_GamersTagsNavigationBehavior?.Clear();
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		list.AddRange(m_PlayerList.Select((NetLobbyPlayerConsoleView player) => player.GamerTagAndName));
		if (list.Any())
		{
			m_GamersTagsNavigationBehavior?.SetEntitiesHorizontal(list);
		}
	}

	private void Launch()
	{
		base.ViewModel.Launch();
	}

	public void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.AddRow<OwlcatButton>(m_SavePartFocusButton);
		List<IConsoleEntity> entities = new List<IConsoleEntity>();
		m_PlayerList.ForEach(delegate(NetLobbyPlayerConsoleView p)
		{
			entities.Add(p);
		});
		if (base.ViewModel.EpicGamesButtonActive.Value)
		{
			entities.Add(m_ConnectEpicGamesToSteam);
		}
		if (entities.Any())
		{
			navigationBehaviour.AddRow(entities);
		}
		AddDisposable(IsInLobbyPart.Subscribe(delegate(bool state)
		{
			if (state)
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					navigationBehaviour.FocusOnEntityManual(m_SavePartFocusButton);
				}, 1);
			}
			else
			{
				navigationBehaviour.UnFocusCurrentEntity();
			}
		}));
		AddDisposable(navigationBehaviour.Focus.Subscribe(OnFocusEntity));
		m_MainNavigationBehavior = navigationBehaviour;
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_SaveSlotIsFocused.Value = entity as OwlcatButton == m_SavePartFocusButton && IsInLobbyPart.Value;
		m_EpicGamesIsFocused.Value = entity as OwlcatButton == m_ConnectEpicGamesToSteam && IsInLobbyPart.Value;
	}
}
