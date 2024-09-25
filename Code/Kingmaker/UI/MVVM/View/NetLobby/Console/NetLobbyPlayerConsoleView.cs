using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbyPlayerConsoleView : NetLobbyPlayerBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	public readonly BoolReactiveProperty IsFocused = new BoolReactiveProperty();

	public void SetFocus(bool value)
	{
		IsFocused.Value = value;
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		if (m_MainButton.Interactable)
		{
			return m_MainButton.IsActive();
		}
		return false;
	}

	public void InvitePlayer()
	{
		if (InviteButtonInteractable.Value)
		{
			base.ViewModel.InviteFromPrimaryStore();
		}
	}

	public void InviteEpicPlayer()
	{
		BoolReactiveProperty epicGamesAuthorized = base.ViewModel.EpicGamesAuthorized;
		if (epicGamesAuthorized == null || !epicGamesAuthorized.Value)
		{
			InvitePlayer();
		}
		else if (base.ViewModel.EpicGamesAuthorized.Value && InviteButtonInteractable.Value)
		{
			base.ViewModel.InviteFromSecondaryStore();
		}
	}

	public void KickPlayer()
	{
		if (KickButtonInteractable.Value && base.ViewModel.IsMeHost.Value)
		{
			base.ViewModel.Kick();
		}
	}

	public void AddPlayerInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action showGamersTagModeAction, BoolReactiveProperty canConfirmLaunch)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InviteEpicPlayer();
		}, 10, InviteButtonInteractable.And(IsFocused).And(base.ViewModel.EpicGamesAuthorized).And(canConfirmLaunch.Not())
			.ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InviteEpicGamesPlayer));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InvitePlayer();
		}, 8, InviteButtonInteractable.And(IsFocused).And(canConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InvitePlayer));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			KickPlayer();
		}, 11, KickButtonInteractable.And(IsFocused).And(base.ViewModel.IsMeHost).And(canConfirmLaunch.Not())
			.ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.KickPlayer));
	}

	public void AddGamerTagInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action hideGamersTagModeAction, BoolReactiveProperty canConfirmLaunch)
	{
		m_GamerTagAndName.AddGamerTagInput(inputLayer, hintsWidget, hideGamersTagModeAction, canConfirmLaunch);
	}
}
