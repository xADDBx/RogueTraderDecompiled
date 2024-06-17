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
	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
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

	public void AddPlayerInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InviteEpicPlayer();
		}, 10, InviteButtonInteractable.And(m_IsFocused).And(base.ViewModel.EpicGamesAuthorized).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InviteEpicGamesPlayer));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InvitePlayer();
		}, 8, InviteButtonInteractable.And(m_IsFocused).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InvitePlayer));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			KickPlayer();
		}, 11, KickButtonInteractable.And(m_IsFocused).And(base.ViewModel.IsMeHost).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.KickPlayer));
	}
}
