using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;
using Kingmaker.Code.UI.MVVM.View.NewGame.PC;
using Kingmaker.Code.UI.MVVM.View.TermOfUse;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.CharGen.PC;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.MainMenu.PC;

public class MainMenuPCView : ViewBase<MainMenuVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected float m_DelayBeforeShow = 1f;

	[SerializeField]
	private MainMenuSideBarPCView m_MainMenuSideBarPCView;

	[SerializeField]
	private TermsOfUsePCView m_TermsOfUsePCView;

	[SerializeField]
	private CreditsBaseView m_CreditsView;

	[SerializeField]
	private NewGamePCView m_NewGamePCView;

	[SerializeField]
	private CharGenContextPCView m_CharGenContextPCView;

	[SerializeField]
	private FeedbackPopupPCView m_FeedbackPopupPCView;

	[SerializeField]
	private FirstLaunchSettingsPCView m_FirstLaunchSettingsPCView;

	[Header("First Time Launch FX")]
	[SerializeField]
	private UIFirstLaunchFX m_FirstLaunchFX;

	public void Initialize()
	{
		m_MainMenuSideBarPCView.Initialize();
		m_TermsOfUsePCView.Initialize();
		m_NewGamePCView.Initialize();
		m_CharGenContextPCView.Initialize();
		m_FirstLaunchSettingsPCView.Initialize();
		m_CreditsView.Initialize();
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		if (m_FadeAnimator.CanvasGroup != null)
		{
			m_FadeAnimator.CanvasGroup.alpha = 1f;
		}
		m_MainMenuSideBarPCView.Bind(base.ViewModel.MainMenuSideBarVM);
		m_CharGenContextPCView.Bind(base.ViewModel.CharGenContextVM);
		AddDisposable(base.ViewModel.NewGameVM.Subscribe(m_NewGamePCView.Bind));
		AddDisposable(base.ViewModel.FeedbackPopupVM.Subscribe(m_FeedbackPopupPCView.Bind));
		AddDisposable(base.ViewModel.TermsOfUseVM.Subscribe(m_TermsOfUsePCView.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsView.Bind));
		AddDisposable(base.ViewModel.FirstLaunchSettings.Subscribe(m_FirstLaunchSettingsPCView.Bind));
		AddDisposable(base.ViewModel.PlayFirstLaunchFXCommand.Subscribe(PlayFirstLaunchFX));
		DelayedInvoker.InvokeInTime(delegate
		{
			m_FadeAnimator.DisappearAnimation();
		}, m_DelayBeforeShow);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void PlayFirstLaunchFX()
	{
		m_FirstLaunchFX.PlayEffect();
	}
}
