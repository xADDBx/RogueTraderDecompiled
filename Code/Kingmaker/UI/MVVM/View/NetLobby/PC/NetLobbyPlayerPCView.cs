using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyPlayerPCView : NetLobbyPlayerBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_KickButton;

	[SerializeField]
	private Image m_InfoPlayerDlcList;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(InviteButtonInteractable.Subscribe(m_MainButton.SetInteractable));
		AddDisposable(KickButtonInteractable.Subscribe(m_KickButton.gameObject.SetActive));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Invite));
		AddDisposable(m_KickButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Kick));
		AddDisposable(base.ViewModel.PlayerDLcList.Subscribe(CheckDlcList));
	}

	private void CheckDlcList(string dlcList)
	{
		if (!(m_InfoPlayerDlcList == null))
		{
			m_InfoPlayerDlcList.gameObject.SetActive(!string.IsNullOrWhiteSpace(dlcList));
			m_InfoPlayerDlcList.SetHint(dlcList);
		}
	}
}
