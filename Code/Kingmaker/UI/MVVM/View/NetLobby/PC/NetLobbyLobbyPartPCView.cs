using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Kingmaker.UI.MVVM.View.NetLobby.PC.DlcList;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyLobbyPartPCView : NetLobbyLobbyPartBaseView
{
	[Header("PC Part")]
	[Space]
	[SerializeField]
	private OwlcatButton m_DisconnectButton;

	[SerializeField]
	private TextMeshProUGUI m_DisconnectButtonText;

	[SerializeField]
	private OwlcatButton m_DlcListButton;

	[SerializeField]
	private TextMeshProUGUI m_DlcListButtonButtonText;

	[SerializeField]
	private OwlcatButton m_LaunchButton;

	[SerializeField]
	private TextMeshProUGUI m_LaunchButtonText;

	[SerializeField]
	private List<NetLobbyPlayerPCView> m_PlayerList;

	[SerializeField]
	private OwlcatButton m_LobbyIdCopyButton;

	[SerializeField]
	private TextMeshProUGUI m_LobbyIdCopyButtonText;

	[SerializeField]
	private OwlcatButton m_LobbyIdShowHideButton;

	[SerializeField]
	private OwlcatButton m_ResetCurrentSave;

	[SerializeField]
	private OwlcatButton m_SaveListBackButton;

	[SerializeField]
	private TextMeshProUGUI m_SaveListBackButtonText;

	[SerializeField]
	private NetLobbyInvitePlayerDifferentPlatformsPCView m_DifferentPlatformsInvitePCView;

	[SerializeField]
	private NetLobbyDlcListPCView m_DlcListPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_LobbyIdCopyButtonText.text = UIStrings.Instance.NetLobbyTexts.CopyLobbyId;
		m_DisconnectButtonText.text = UIStrings.Instance.NetLobbyTexts.DisconnectLobby;
		m_DlcListButtonButtonText.text = UIStrings.Instance.NetLobbyTexts.DlcList;
		m_SaveListBackButtonText.text = UIStrings.Instance.CommonTexts.Back;
		m_DifferentPlatformsInvitePCView.Initialize();
		m_DlcListPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsInRoom.CombineLatest(base.ViewModel.NetGameCurrentState, base.ViewModel.SaveSlotCollectionVm, base.ViewModel.IsHost, (bool inRoom, NetGame.State state, SaveSlotCollectionVM collection, bool host) => new { inRoom, state, collection, host }).Subscribe(value =>
		{
			m_LaunchButton.gameObject.SetActive(value.host);
		}));
		AddDisposable(LaunchButtonText.Subscribe(delegate(string value)
		{
			UISounds.Instance.SetClickSound(m_LaunchButton, IsLaunchSound ? UISounds.ButtonSoundsEnum.NoSound : UISounds.ButtonSoundsEnum.NormalSound);
			m_LaunchButtonText.text = value;
		}));
		AddDisposable(base.ViewModel.IsSaveTransfer.Subscribe(delegate(bool value)
		{
			m_LaunchButton.SetInteractable(!value && LaunchButtonActive.Value);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_DisconnectButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Disconnect("DisconnectButton");
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_DlcListButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowDlcList();
		}));
		AddDisposable(LaunchButtonInteractable.Subscribe(m_LaunchButton.SetInteractable));
		AddDisposable(m_LaunchButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			if (!LaunchButtonInteractable.Value)
			{
				base.ViewModel.Launch();
			}
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_LaunchButton.OnLeftClickAsObservable(), delegate
		{
			m_LaunchButton.SetInteractable(!base.ViewModel.Launch());
		}));
		for (int i = 0; i < m_PlayerList.Count; i++)
		{
			m_PlayerList[i].Bind(base.ViewModel.PlayerVms[i]);
		}
		AddDisposable(ObservableExtensions.Subscribe(m_LobbyIdCopyButton.OnLeftClickAsObservable(), delegate
		{
			CopyLobbyId();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_LobbyIdShowHideButton.OnLeftClickAsObservable(), delegate
		{
			ShowHideLobbyId();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_EmptySaveSlotButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ChooseSave();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_ResetCurrentSave.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ResetCurrentSave();
		}));
		AddDisposable(ResetCurrentSaveActive.Subscribe(m_ResetCurrentSave.gameObject.SetActive));
		AddDisposable(m_ResetCurrentSave.SetHint(UIStrings.Instance.NetLobbyTexts.ResetCurrentSave));
		AddDisposable(ObservableExtensions.Subscribe(m_SaveListBackButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_ConnectEpicGamesToSteam.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenEpicGamesLayer();
		}));
		AddDisposable(m_LobbyIdShowHideButton.SetHint(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode));
		AddDisposable(base.ViewModel.DifferentPlatformInviteVM.Subscribe(m_DifferentPlatformsInvitePCView.Bind));
		AddDisposable(base.ViewModel.DlcListVM.Subscribe(m_DlcListPCView.Bind));
	}

	private void TrySetHints()
	{
		if (!base.ViewModel.IsSaveAllowed.Value)
		{
			AddDisposable(m_LaunchButton.SetHint(UIStrings.Instance.NetLobbyTexts.ImpossibleToStartCoopGameInThisMoment));
			AddDisposable(m_EmptySaveSlotButton.SetHint(UIStrings.Instance.NetLobbyTexts.ImpossibleToStartCoopGameInThisMoment));
		}
	}
}
