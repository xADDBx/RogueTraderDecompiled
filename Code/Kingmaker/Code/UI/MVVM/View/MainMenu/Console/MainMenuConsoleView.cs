using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;
using Kingmaker.Code.UI.MVVM.View.NewGame.Console;
using Kingmaker.Code.UI.MVVM.View.TermOfUse.Console;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.CharGen.Console;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.MainMenu.Console;

public class MainMenuConsoleView : ViewBase<MainMenuVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected float m_DelayBeforeShow = 1f;

	[SerializeField]
	private MainMenuSideBarConsoleView m_MenuSideBarConsoleView;

	[SerializeField]
	private TermsOfUseConsoleView m_TermsOfUseConsoleView;

	[SerializeField]
	private CreditsBaseView m_CreditsView;

	[SerializeField]
	private CharGenContextConsoleView m_CharGenContextConsoleView;

	[SerializeField]
	private NewGameConsoleView m_NewGameConsoleView;

	[SerializeField]
	private FirstLaunchSettingsConsoleView m_FirstLaunchSettingsConsoleView;

	[SerializeField]
	private ConsoleCursor m_ConsoleCursor;

	[Header("First Time Launch FX")]
	[SerializeField]
	private UIFirstLaunchFX m_FirstLaunchFX;

	public void Initialize()
	{
		m_TermsOfUseConsoleView.Initialize();
		m_CharGenContextConsoleView.Initialize();
		m_NewGameConsoleView.Initialize();
		m_FirstLaunchSettingsConsoleView.Initialize();
		m_CreditsView.Initialize();
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		if (m_FadeAnimator.CanvasGroup != null)
		{
			m_FadeAnimator.CanvasGroup.alpha = 1f;
		}
		m_MenuSideBarConsoleView.Bind(base.ViewModel.MainMenuSideBarVM);
		m_CharGenContextConsoleView.Bind(base.ViewModel.CharGenContextVM);
		AddDisposable(base.ViewModel.NewGameVM.Subscribe(m_NewGameConsoleView.Bind));
		AddDisposable(base.ViewModel.TermsOfUseVM.Subscribe(m_TermsOfUseConsoleView.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsView.Bind));
		AddDisposable(base.ViewModel.FirstLaunchSettings.Subscribe(m_FirstLaunchSettingsConsoleView.Bind));
		AddDisposable(base.ViewModel.PlayFirstLaunchFXCommand.Subscribe(PlayFirstLaunchFX));
		AddDisposable(m_ConsoleCursor.Bind());
		m_ConsoleCursor.SetActive(active: false);
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
