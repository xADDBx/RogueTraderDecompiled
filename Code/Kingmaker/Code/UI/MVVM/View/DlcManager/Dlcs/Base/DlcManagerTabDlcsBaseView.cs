using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;

public class DlcManagerTabDlcsBaseView : ViewBase<DlcManagerTabDlcsVM>
{
	[Header("Texts")]
	[SerializeField]
	protected TextMeshProUGUI m_DlcDescription;

	[SerializeField]
	private List<Image> m_DlcDescriptionBackgroundGradients;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Art")]
	[SerializeField]
	private Image m_DlcArt;

	[Header("Bottom Block")]
	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveThisDlc;

	[SerializeField]
	protected OwlcatButton m_PurchaseButton;

	[SerializeField]
	private TextMeshProUGUI m_PurchaseLabel;

	[SerializeField]
	private TextMeshProUGUI m_PurchasedLabel;

	[SerializeField]
	private TextMeshProUGUI m_ComingSoonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DownloadingInProgressText;

	[SerializeField]
	protected OwlcatButton m_InstallButton;

	[SerializeField]
	private TextMeshProUGUI m_InstallButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcIsBoughtAndNotInstalledText;

	[Header("Dlcs Block")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRectDlcs;

	protected bool IsInit;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		ScrollToTop();
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			base.ViewModel.ResetVideo();
			if (value)
			{
				ScrollToTop();
				UpdateDlcEntities();
			}
		}));
		AddDisposable(base.ViewModel.ChangeStory.Subscribe(delegate
		{
			bool active = base.ViewModel.Art.Value != null && base.ViewModel.Video?.Value == null;
			m_DlcArt.gameObject.SetActive(active);
			if (base.ViewModel.Art.Value != null)
			{
				m_DlcArt.sprite = base.ViewModel.Art.Value;
			}
			VideoClip videoClip = base.ViewModel.Video?.Value;
			ShowHideVideo(videoClip != null);
			base.ViewModel.CustomUIVideoPlayerVM.SetVideo(videoClip, base.ViewModel.Art.Value, base.ViewModel.SoundStart.Value, base.ViewModel.SoundStop.Value);
			m_DlcDescription.text = base.ViewModel.Description.Value;
			m_ScrollRect.Or(null)?.ScrollToTop();
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
		}));
		AddDisposable((from value in m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable()
			select 1f - value).Subscribe(delegate(float invertedValue)
		{
			foreach (Image dlcDescriptionBackgroundGradient in m_DlcDescriptionBackgroundGradients)
			{
				Color color = dlcDescriptionBackgroundGradient.color;
				color.a = ((!m_ScrollRect.verticalScrollbar.gameObject.activeSelf) ? 0f : invertedValue);
				dlcDescriptionBackgroundGradient.color = color;
			}
		}));
		AddDisposable(base.ViewModel.DownloadingInProgress.CombineLatest(base.ViewModel.DlcIsBoughtAndNotInstalled, (bool downloadInProgress, bool boughtAndNotInstalled) => new { downloadInProgress, boughtAndNotInstalled }).Subscribe(value =>
		{
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
			CheckInstallState(value.downloadInProgress, value.boughtAndNotInstalled);
		}));
		CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
		CheckInstallState(base.ViewModel.DownloadingInProgress.Value, base.ViewModel.DlcIsBoughtAndNotInstalled.Value);
		m_YouDontHaveThisDlc.text = UIStrings.Instance.DlcManager.AvailableForPurchase;
		m_PurchaseLabel.text = UIStrings.Instance.DlcManager.Purchase;
		m_PurchasedLabel.text = UIStrings.Instance.DlcManager.Purchased;
		m_ComingSoonLabel.text = UIStrings.Instance.DlcManager.ComingSoon;
		m_DownloadingInProgressText.text = UIStrings.Instance.DlcManager.DlcDownloading;
		m_DlcIsBoughtAndNotInstalledText.text = UIStrings.Instance.DlcManager.DlcBoughtAndNotInstalled;
		m_InstallButtonLabel.text = UIStrings.Instance.DlcManager.Install;
		SetTextFontSize(base.ViewModel.FontMultiplier);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void ShowHideVideo(bool state)
	{
		ShowHideVideoImpl(state);
	}

	protected virtual void ShowHideVideoImpl(bool state)
	{
	}

	private void UpdateDlcEntities()
	{
		UpdateDlcEntitiesImpl();
	}

	protected virtual void UpdateDlcEntitiesImpl()
	{
	}

	private void CheckAvailableState(bool canPurchase, bool available)
	{
		if (base.ViewModel.DownloadingInProgress.Value || base.ViewModel.DlcIsBoughtAndNotInstalled.Value)
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(value: false);
			m_PurchaseButton.gameObject.SetActive(value: false);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(value: false);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(!available && canPurchase);
			m_PurchaseButton.gameObject.SetActive(!available && canPurchase);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(available && canPurchase);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(!available && !canPurchase);
			CheckAvailableStateImpl(canPurchase, available);
		}
	}

	protected virtual void CheckAvailableStateImpl(bool canPurchase, bool available)
	{
	}

	private void CheckInstallState(bool downloadingInProgress, bool boughtAndNotInstalled)
	{
		m_DownloadingInProgressText.transform.parent.gameObject.SetActive(downloadingInProgress && !boughtAndNotInstalled);
		m_DlcIsBoughtAndNotInstalledText.transform.parent.gameObject.SetActive(!downloadingInProgress && boughtAndNotInstalled);
		m_InstallButton.gameObject.SetActive(!downloadingInProgress && boughtAndNotInstalled);
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
		m_ScrollRectDlcs.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectDlcs.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	protected virtual void SetTextFontSize(float multiplier)
	{
	}
}
