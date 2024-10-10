using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Console;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerTabDlcsConsoleView : DlcManagerTabDlcsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabDlcsDlcSelectorConsoleView m_DlcSelectorConsoleView;

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
		m_DlcSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
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

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint purchaseHint, ConsoleHint installHint, ConsoleHint deleteDlcHint, ConsoleHint playPauseVideoHint)
	{
		if (m_ScrollStoryHint != null)
		{
			AddDisposable(m_ScrollStoryHint.BindCustomAction(3, inputLayer, base.ViewModel.IsEnabled));
		}
		if (m_PurchaseHint != null)
		{
			AddDisposable(m_PurchaseHint.BindCustomAction(10, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(base.ViewModel.DlcIsAvailableToPurchase).ToReactiveProperty()));
		}
		if (m_InstallHint != null)
		{
			AddDisposable(m_InstallHint.BindCustomAction(11, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
				.And(base.ViewModel.IsRealConsole)
				.ToReactiveProperty()));
		}
		AddDisposable(purchaseHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowInStore();
		}, 10, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(base.ViewModel.DlcIsAvailableToPurchase).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		purchaseHint.SetLabel(UIStrings.Instance.DlcManager.Purchase);
		AddDisposable(installHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.InstallDlc();
		}, 11, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.ToReactiveProperty())));
		installHint.SetLabel(UIStrings.Instance.DlcManager.Install);
		AddDisposable(deleteDlcHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.DeleteDlc();
		}, 11, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought).And(base.ViewModel.DlcIsBoughtAndNotInstalled.Not()).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.And(base.ViewModel.IsEditionDlc.Not())
			.ToReactiveProperty())));
		deleteDlcHint.SetLabel(UIStrings.Instance.DlcManager.DeleteDlc);
		m_CustomUIVideoPlayerConsoleView.CreateInputImpl(inputLayer, hintsWidget, playPauseVideoHint, base.ViewModel.IsEnabled);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_DlcSelectorConsoleView.GetNavigationEntities();
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerConsoleView.gameObject.SetActive(state);
	}

	protected override void UpdateDlcEntitiesImpl()
	{
		base.UpdateDlcEntitiesImpl();
		m_DlcSelectorConsoleView.UpdateDlcEntities();
	}
}
