using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetRoles.PC;

public class NetRolesPCView : NetRolesBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private List<NetRolesPlayerPCView> m_Players;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_ApplyButton;

	[SerializeField]
	private TextMeshProUGUI m_ApplyLabel;

	public override void Initialize()
	{
		base.Initialize();
		m_Players.ForEach(delegate(NetRolesPlayerPCView p)
		{
			p.Initialize();
		});
	}

	protected override void BindViewImplementation()
	{
		for (int i = 0; i < m_Players.Count; i++)
		{
			m_Players[i].Bind((base.ViewModel.PlayerVms.Count > i) ? base.ViewModel.PlayerVms[i] : null);
		}
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		m_ApplyLabel.text = (base.ViewModel.IsRoomOwner ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.CommonTexts.CloseWindow);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose));
		AddDisposable(m_ApplyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose));
		base.BindViewImplementation();
	}
}
