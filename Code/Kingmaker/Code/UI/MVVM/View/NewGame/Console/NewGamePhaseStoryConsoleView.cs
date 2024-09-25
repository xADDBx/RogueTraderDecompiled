using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Console;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseStoryConsoleView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorConsoleView m_StorySelectorConsoleView;

	[SerializeField]
	private CustomUIVideoPlayerConsoleView m_CustomUIVideoPlayerConsoleView;

	[SerializeField]
	private ConsoleHint m_ScrollStoryHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_InstallHint;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerConsoleView.Initialize();
			IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_CustomUIVideoPlayerConsoleView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.BindViewImplementation();
		m_StorySelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint switchOnOffHint, ConsoleHint purchaseHint, ConsoleHint installHint, ConsoleHint deleteDlcHint, ConsoleHint playPauseVideoHint)
	{
		if (m_ScrollStoryHint != null)
		{
			AddDisposable(m_ScrollStoryHint.BindCustomAction(3, inputLayer, base.ViewModel.IsEnabled));
		}
		if (m_PurchaseHint != null)
		{
			AddDisposable(m_PurchaseHint.BindCustomAction(10, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(SwitchOnButtonActive.Not()).ToReactiveProperty()));
		}
		if (m_InstallHint != null)
		{
			AddDisposable(m_InstallHint.BindCustomAction(11, inputLayer, base.ViewModel.IsEnabled.And(SwitchOnButtonActive.Not()).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
				.And(base.ViewModel.IsRealConsole)
				.ToReactiveProperty()));
		}
		AddDisposable(switchOnOffHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.SwitchDlcOn();
		}, 10, base.ViewModel.IsEnabled.And(SwitchOnButtonActive).ToReactiveProperty())));
		switchOnOffHint.SetLabel(UIStrings.Instance.SettingsUI.SettingsToggleOff);
		AddDisposable(installHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.InstallDlc();
		}, 11, base.ViewModel.IsEnabled.And(SwitchOnButtonActive.Not()).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.ToReactiveProperty())));
		installHint.SetLabel(UIStrings.Instance.DlcManager.Install);
		AddDisposable(deleteDlcHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.DeleteDlc();
		}, 11, base.ViewModel.IsEnabled.And(SwitchOnButtonActive).And(base.ViewModel.DlcIsBoughtAndNotInstalled.Not()).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.ToReactiveProperty())));
		deleteDlcHint.SetLabel(UIStrings.Instance.DlcManager.DeleteDlc);
		AddDisposable(base.ViewModel.DlcIsOn.Subscribe(delegate(bool value)
		{
			switchOnOffHint.SetLabel(value ? UIStrings.Instance.SettingsUI.SettingsToggleOff : UIStrings.Instance.SettingsUI.SettingsToggleOn);
		}));
		AddDisposable(purchaseHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowInStore();
		}, 10, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(SwitchOnButtonActive.Not()).ToReactiveProperty())));
		purchaseHint.SetLabel(UIStrings.Instance.DlcManager.Purchase);
		m_CustomUIVideoPlayerConsoleView.CreateInputImpl(inputLayer, hintsWidget, playPauseVideoHint, base.ViewModel.IsEnabled);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_StorySelectorConsoleView.GetNavigationEntities();
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerConsoleView.gameObject.SetActive(state);
	}
}
