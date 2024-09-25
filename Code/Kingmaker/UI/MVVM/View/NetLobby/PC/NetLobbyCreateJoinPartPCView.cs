using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyCreateJoinPartPCView : NetLobbyCreateJoinPartBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_CreateLobbyButton;

	[SerializeField]
	private TextMeshProUGUI m_CreateLobbyButtonText;

	[Space]
	[SerializeField]
	private OwlcatButton m_JoinLobbyButton;

	[SerializeField]
	private TextMeshProUGUI m_JoinLobbyButtonText;

	[SerializeField]
	private OwlcatButton m_ShowHideLobbyCodeButton;

	[SerializeField]
	private OwlcatButton m_LobbyIdPasteButton;

	public override void Initialize()
	{
		base.Initialize();
		m_CreateLobbyButtonText.text = UIStrings.Instance.NetLobbyTexts.CreateLobby;
		m_JoinLobbyButtonText.text = UIStrings.Instance.NetLobbyTexts.JoinLobby;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.RegionDropdownVM.Subscribe(delegate(OwlcatDropdownVM value)
		{
			m_CreateLobbyButton.SetInteractable(value != null);
		}));
		AddDisposable(ReadyToJoin.Subscribe(m_JoinLobbyButton.SetInteractable));
		AddDisposable(ObservableExtensions.Subscribe(m_LobbyIdPasteButton.OnLeftClickAsObservable(), delegate
		{
			m_LobbyCodeInputField.text = base.ViewModel.GetCopiedLobbyId();
		}));
		AddDisposable(m_CreateLobbyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.CreateLobby));
		AddDisposable(m_JoinLobbyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.JoinLobby));
		AddDisposable(UniRxExtensionMethods.Subscribe(m_ShowHideLobbyCodeButton.OnLeftClickAsObservable(), delegate
		{
			ShowLobbyCode.Value = !ShowLobbyCode.Value;
		}));
		AddDisposable(m_LobbyIdPasteButton.SetHint(UIStrings.Instance.NetLobbyTexts.PasteLobbyId));
		AddDisposable(m_ShowHideLobbyCodeButton.SetHint(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode));
	}
}
