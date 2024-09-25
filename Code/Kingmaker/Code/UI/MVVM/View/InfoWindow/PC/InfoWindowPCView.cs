using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow.PC;

public class InfoWindowPCView : InfoWindowBaseView
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private Vector2 m_DefaultPosition;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.Close));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		Hide();
		m_IsStartPosition = false;
	}

	protected override void SetPosition()
	{
		base.transform.localPosition = (m_IsStartPosition ? m_Position : m_DefaultPosition);
	}

	protected override void OnClose()
	{
		base.OnClose();
		base.ViewModel.OnClose();
	}
}
