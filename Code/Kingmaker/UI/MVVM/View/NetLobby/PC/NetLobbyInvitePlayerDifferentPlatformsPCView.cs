using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyInvitePlayerDifferentPlatformsPCView : NetLobbyInvitePlayerDifferentPlatformsBaseView
{
	[SerializeField]
	private OwlcatButton m_InviteFromSteamButton;

	[SerializeField]
	private TextMeshProUGUI m_InviteFromSteamLabel;

	[SerializeField]
	private OwlcatButton m_InviteFromEpicGamesButton;

	[SerializeField]
	private TextMeshProUGUI m_InviteFromEpicGamesLabel;

	[SerializeField]
	private OwlcatButton m_CancelInviteButton;

	[SerializeField]
	private TextMeshProUGUI m_CancelInviteLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	public override void Initialize()
	{
		base.Initialize();
		m_InviteFromSteamLabel.text = UIStrings.Instance.NetLobbyTexts.InvitePlayer;
		m_InviteFromEpicGamesLabel.text = UIStrings.Instance.NetLobbyTexts.InviteEpicGamesPlayer;
		m_CancelInviteLabel.text = UIStrings.Instance.CommonTexts.Cancel;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_InviteFromSteamButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnInvitePlayer();
		}));
		AddDisposable(m_InviteFromEpicGamesButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnInviteEpicGamesPlayer();
		}));
		AddDisposable(m_CancelInviteButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClose();
		}));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnClose();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose));
	}
}
