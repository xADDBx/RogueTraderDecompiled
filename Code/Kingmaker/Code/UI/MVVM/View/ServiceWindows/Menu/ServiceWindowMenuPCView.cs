using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Menu;

public class ServiceWindowMenuPCView : ViewBase<ServiceWindowsMenuVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private ServiceWindowMenuSelectorPCView m_MenuSelector;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private GameObject m_AdditionalBackground;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_MenuSelector.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_Animator.AppearAnimation();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
		if (m_AdditionalBackground != null)
		{
			AddDisposable(base.ViewModel.IsAdditionalBackgroundNeeded.Subscribe(m_AdditionalBackground.SetActive));
		}
		m_CloseButton.gameObject.SetActive(Game.Instance.IsControllerMouse);
	}

	protected override void DestroyViewImplementation()
	{
		m_Animator.DisappearAnimation();
	}
}
