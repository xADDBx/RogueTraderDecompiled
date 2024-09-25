using System;
using Kingmaker.Blueprints.Root.Strings;
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

	private IDisposable m_CurrentPlayerDlcsDisposable;

	private IDisposable m_ProblemsWithPlayerAndHostDlcsDisposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(InviteButtonInteractable.Subscribe(m_MainButton.SetInteractable));
		AddDisposable(KickButtonInteractable.Subscribe(m_KickButton.gameObject.SetActive));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Invite));
		AddDisposable(m_KickButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Kick));
		AddDisposable(base.ViewModel.PlayerDLcStringList.Subscribe(CheckCurrentPlayerDlcsNamesList));
	}

	protected override void DestroyViewImplementation()
	{
		m_CurrentPlayerDlcsDisposable?.Dispose();
		m_CurrentPlayerDlcsDisposable = null;
		m_ProblemsWithPlayerAndHostDlcsDisposable?.Dispose();
		m_ProblemsWithPlayerAndHostDlcsDisposable = null;
		base.DestroyViewImplementation();
	}

	private void CheckCurrentPlayerDlcsNamesList(string dlcList)
	{
		if (!(m_InfoPlayerDlcList == null))
		{
			bool flag = !string.IsNullOrWhiteSpace(dlcList);
			m_InfoPlayerDlcList.gameObject.SetActive(flag);
			m_CurrentPlayerDlcsDisposable?.Dispose();
			m_CurrentPlayerDlcsDisposable = null;
			if (flag)
			{
				m_CurrentPlayerDlcsDisposable = m_InfoPlayerDlcList.SetHint(dlcList);
			}
		}
	}

	protected override void CheckProblemsWithPlayerAndHostDlcsImpl(string dlcList)
	{
		base.CheckProblemsWithPlayerAndHostDlcsImpl(dlcList);
		m_ProblemsWithPlayerAndHostDlcsDisposable?.Dispose();
		m_ProblemsWithPlayerAndHostDlcsDisposable = null;
		if (!string.IsNullOrWhiteSpace(dlcList))
		{
			m_ProblemsWithPlayerAndHostDlcsDisposable = m_ProblemsWithPlayerAndHostDlcsMarker.SetHint(UIStrings.Instance.NetLobbyTexts.PlayerHasNoDlcs.Text + ":" + Environment.NewLine + Environment.NewLine + dlcList);
		}
	}
}
