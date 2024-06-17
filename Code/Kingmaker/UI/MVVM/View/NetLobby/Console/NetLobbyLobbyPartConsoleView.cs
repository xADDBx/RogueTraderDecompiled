using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
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

	private IConsoleHint m_ContinueHint;

	private readonly BoolReactiveProperty m_SaveSlotIsFocused = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_EpicGamesIsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		for (int i = 0; i < m_PlayerList.Count; i++)
		{
			m_PlayerList[i].Bind(base.ViewModel.PlayerVms[i]);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_SaveSlotIsFocused.Value = false;
		base.DestroyViewImplementation();
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.Disconnect();
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState.Not()).ToReactiveProperty()), UIStrings.Instance.CharGen.Back));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.Disconnect();
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.DisconnectLobby));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsPlayingState).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowNetLobbyTutorial();
		}, 19, base.ViewModel.IsAnyTutorialBlocks.And(IsInLobbyPart).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.HowToPlay));
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
		}, 11, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost).And(base.ViewModel.IsSaveAllowed)
			.And(m_SaveSlotIsFocused)
			.And(ResetCurrentSaveActive)
			.ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.ResetCurrentSave));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ChooseSave();
		}, 8, IsInLobbyPart.And(LaunchButtonActive.Not()).And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost)
			.And(base.ViewModel.IsSaveAllowed)
			.And(m_SaveSlotIsFocused)
			.ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ChooseSave();
		}, 10, IsInLobbyPart.And(base.ViewModel.CanConfirmLaunch.Not()).And(base.ViewModel.IsHost).And(base.ViewModel.IsSaveAllowed)
			.And(m_SaveSlotIsFocused)
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
			.And(base.ViewModel.IsSaveAllowed)
			.And(base.ViewModel.NeedReconnect.Not())
			.ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_ContinueHint.SetLabel(UIStrings.Instance.NetLobbyTexts.Launch);
		AddDisposable(LaunchButtonText.Subscribe(m_ContinueHint.SetLabel));
		m_PlayerList.ForEach(delegate(NetLobbyPlayerConsoleView p)
		{
			p.AddPlayerInput(inputLayer, hintsWidget);
		});
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
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_SaveSlotIsFocused.Value = entity as OwlcatButton == m_SavePartFocusButton && IsInLobbyPart.Value;
		m_EpicGamesIsFocused.Value = entity as OwlcatButton == m_ConnectEpicGamesToSteam && IsInLobbyPart.Value;
	}
}
