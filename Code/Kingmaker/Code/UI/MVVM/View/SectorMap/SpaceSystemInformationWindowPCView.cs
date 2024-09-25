using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class SpaceSystemInformationWindowPCView : SpaceSystemInformationWindowBaseView
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	private bool m_EscIsSubscribed;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.CloseInformationWindow));
	}

	protected override void ShowHideWindow(bool state)
	{
		if (!state)
		{
			EscHotkeyManager.Instance.Unsubscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = false;
		}
		base.ShowHideWindow(state);
		if (state)
		{
			EscHotkeyManager.Instance.Subscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = true;
		}
	}

	protected override void DestroyViewImplementation()
	{
		if (m_EscIsSubscribed)
		{
			EscHotkeyManager.Instance.Unsubscribe(base.CloseInformationWindow);
			m_EscIsSubscribed = false;
		}
		base.DestroyViewImplementation();
	}
}
