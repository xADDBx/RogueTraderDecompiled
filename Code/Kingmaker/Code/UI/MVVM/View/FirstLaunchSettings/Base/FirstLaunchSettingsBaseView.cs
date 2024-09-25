using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Legacy.MainMenuUI;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;

public abstract class FirstLaunchSettingsBaseView : ViewBase<FirstLaunchSettingsVM>
{
	[Header("Common")]
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private CanvasGroup m_AdditionalCanvasGroup;

	protected readonly BoolReactiveProperty IsNotOnLanguagePage = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty IsVisibleContinueButton = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty IsVisibleFinishButton = new BoolReactiveProperty();

	private bool m_IsLanguagePage;

	protected bool IsFocusedOnLanguageItem;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	[SerializeField]
	private SplashScreenController.ScreenUnit m_PhotosensitivityUnit;

	private Sequence m_TweenSequence;

	public void Initialize()
	{
		m_Animator.Initialize();
		InitializeImpl();
	}

	protected override void BindViewImplementation()
	{
		BuildNavigation();
		SetupTexts();
		AddDisposable(base.ViewModel.AccessiabilityPageVM.Subscribe(delegate(FirstLaunchAccessiabilityPageVM vm)
		{
			SetContinueButtonVisibility(vm == null);
		}));
		AddDisposable(base.ViewModel.LanguagePageVM.Subscribe(delegate(FirstLaunchLanguagePageVM vm)
		{
			OnLanguagePage(vm != null);
		}));
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
		AddDisposable(base.ViewModel.LanguageChanged.Subscribe(SetupTexts));
		AddDisposable(base.ViewModel.ShowPhotosensitivityScreen.Subscribe(ShowPhotoSensitivityScreen));
		Show();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	protected virtual void InitializeImpl()
	{
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "FirstLaunchSettingsBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	protected void ConfirmAction()
	{
		if (m_IsLanguagePage && IsFocusedOnLanguageItem)
		{
			m_NavigationBehaviour.CurrentEntity.OnConfirmClick();
		}
		base.ViewModel.NextPage();
	}

	protected void DeclineAction()
	{
		if (!m_IsLanguagePage)
		{
			base.ViewModel.PreviousPage();
		}
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.FirstLaunchSettings);
		});
		UISounds.Instance.Sounds.Settings.SettingsOpen.Play();
	}

	private void Hide()
	{
		m_Animator.DisappearAnimation();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.FirstLaunchSettings);
		});
		UISounds.Instance.Sounds.Settings.SettingsClose.Play();
	}

	private void OnLanguagePage(bool value)
	{
		m_IsLanguagePage = value;
		IsNotOnLanguagePage.Value = !value;
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		IsFocusedOnLanguageItem = entity is FirstLaunchEntityLanguageItemBaseView;
		IsNotOnLanguagePage.Value = !m_IsLanguagePage;
		SetupTexts();
	}

	private void SetContinueButtonVisibility(bool isVisible)
	{
		IsVisibleContinueButton.Value = isVisible;
		IsVisibleFinishButton.Value = !isVisible;
	}

	protected virtual void SetupTexts()
	{
	}

	protected virtual void ShowPhotoSensitivityScreen()
	{
		if (m_PhotosensitivityUnit.CanvasGroup == null)
		{
			OnComplete();
			return;
		}
		m_TweenSequence = DOTween.Sequence();
		m_TweenSequence.AppendInterval(0.1f);
		UIUtilityShowSplashScreen.ShowSplashScreen(m_PhotosensitivityUnit, m_TweenSequence, base.gameObject, m_AdditionalCanvasGroup);
		m_TweenSequence.AppendCallback(OnComplete);
	}

	private void OnComplete()
	{
		SoundBanksManager.UnloadBank(UIConsts.SplashScreens);
		SoundState.Instance.ResetState(SoundStateType.MainMenu);
		base.ViewModel.Close();
	}
}
