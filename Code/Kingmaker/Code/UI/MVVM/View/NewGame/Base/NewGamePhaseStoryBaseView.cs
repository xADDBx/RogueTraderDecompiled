using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGamePhaseStoryBaseView : ViewBase<NewGamePhaseStoryVM>
{
	[Header("Common")]
	[SerializeField]
	private PantographView m_PantographView;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_StoryDescription;

	[SerializeField]
	private List<Image> m_StoryDescriptionBackgroundGradients;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Art")]
	[SerializeField]
	private Image m_StoryArt;

	[Header("Bottom Block")]
	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveThisDlc;

	[SerializeField]
	private TextMeshProUGUI m_DlcStatusLabel;

	[SerializeField]
	protected OwlcatButton m_PurchaseButton;

	[SerializeField]
	private TextMeshProUGUI m_PurchaseLabel;

	[SerializeField]
	private TextMeshProUGUI m_PurchasedLabel;

	[SerializeField]
	private TextMeshProUGUI m_ComingSoonLabel;

	[SerializeField]
	protected OwlcatMultiButton m_SwitchOnDlcButton;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	[SerializeField]
	private TextMeshProUGUI m_DownloadingInProgressText;

	[SerializeField]
	protected OwlcatButton m_InstallButton;

	[SerializeField]
	private TextMeshProUGUI m_InstallButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_DlcIsBoughtAndNotInstalledText;

	protected readonly BoolReactiveProperty SwitchOnButtonActive = new BoolReactiveProperty();

	protected bool IsInit;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				base.ViewModel.ResetVideo();
				ScrollToTop();
				m_PantographView.Show();
			}
			else
			{
				m_PantographView.Hide();
			}
		}));
		AddDisposable(m_PantographView);
		AddDisposable(base.ViewModel.ChangeStory.Subscribe(delegate
		{
			bool active = base.ViewModel.Art.Value != null && base.ViewModel.Video?.Value == null;
			m_StoryArt.gameObject.SetActive(active);
			if (base.ViewModel.Art.Value != null)
			{
				m_StoryArt.sprite = base.ViewModel.Art.Value;
			}
			VideoClip videoClip = base.ViewModel.Video?.Value;
			ShowHideVideo(videoClip != null);
			base.ViewModel.CustomUIVideoPlayerVM.SetVideo(videoClip, base.ViewModel.Art.Value, base.ViewModel.SoundStart.Value, base.ViewModel.SoundStop.Value);
			m_StoryDescription.text = base.ViewModel.Description.Value;
			ScrollToTop();
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
		}));
		AddDisposable(base.ViewModel.DlcIsOn.Subscribe(delegate(bool value)
		{
			m_SwitchOnDlcButton.SetActiveLayer(value ? "On" : "Off");
		}));
		AddDisposable(m_SwitchOnDlcButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SwitchDlcOn();
		}));
		AddDisposable(m_DlcStatusLabel.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.DlcManager.DlcSwitchOnOffHint)));
		AddDisposable((from value in m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable()
			select 1f - value).Subscribe(delegate(float invertedValue)
		{
			foreach (Image storyDescriptionBackgroundGradient in m_StoryDescriptionBackgroundGradients)
			{
				Color color = storyDescriptionBackgroundGradient.color;
				color.a = ((!m_ScrollRect.verticalScrollbar.gameObject.activeSelf) ? 0f : invertedValue);
				storyDescriptionBackgroundGradient.color = color;
			}
		}));
		AddDisposable(base.ViewModel.DownloadingInProgress.CombineLatest(base.ViewModel.DlcIsBoughtAndNotInstalled, (bool downloadInProgress, bool boughtAndNotInstalled) => new { downloadInProgress, boughtAndNotInstalled }).Subscribe(value =>
		{
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
			CheckInstallState(value.downloadInProgress, value.boughtAndNotInstalled);
		}));
		CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
		CheckInstallState(base.ViewModel.DownloadingInProgress.Value, base.ViewModel.DlcIsBoughtAndNotInstalled.Value);
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
		m_DlcStatusLabel.text = UIStrings.Instance.DlcManager.DlcStatus;
		m_PurchaseLabel.text = UIStrings.Instance.DlcManager.Purchase;
		m_PurchasedLabel.text = UIStrings.Instance.DlcManager.Purchased;
		m_ComingSoonLabel.text = UIStrings.Instance.DlcManager.ComingSoon;
		m_DownloadingInProgressText.text = UIStrings.Instance.DlcManager.DlcDownloading;
		m_DlcIsBoughtAndNotInstalledText.text = UIStrings.Instance.DlcManager.DlcBoughtAndNotInstalled;
		m_InstallButtonLabel.text = UIStrings.Instance.DlcManager.Install;
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

	private void CheckAvailableState(bool canPurchase, bool available)
	{
		if (base.ViewModel.DownloadingInProgress.Value || base.ViewModel.DlcIsBoughtAndNotInstalled.Value)
		{
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(value: false);
			m_PurchaseButton.gameObject.SetActive(value: false);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(value: false);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(value: false);
			m_SwitchOnDlcButton.transform.parent.gameObject.SetActive(value: false);
			SwitchOnButtonActive.Value = false;
		}
		else
		{
			UIDlcManager dlcManager = UIStrings.Instance.DlcManager;
			m_YouDontHaveThisDlc.text = ((base.ViewModel.BlueprintDlc == null) ? ((!base.ViewModel.IsNextButtonAvailable.Value) ? ((string)dlcManager.YouDontHaveThisDlc) : string.Empty) : ((!available && canPurchase) ? ((string)dlcManager.YouDontHaveThisDlc) : ((!base.ViewModel.IsNextButtonAvailable.Value) ? string.Format(dlcManager.YouDontHaveThisStory, base.ViewModel.CampaignName.Value) : string.Empty)));
			m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive((!available && canPurchase) || !base.ViewModel.IsNextButtonAvailable.Value);
			m_PurchaseButton.gameObject.SetActive(!available && canPurchase);
			m_PurchasedLabel.transform.parent.gameObject.SetActive(available && canPurchase);
			m_ComingSoonLabel.transform.parent.gameObject.SetActive(!available && !canPurchase);
			CheckAvailableStateImpl(available);
			SwitchOnButtonActive.Value = available && base.ViewModel.BlueprintDlc != null && base.ViewModel.IsNextButtonAvailable.Value;
			m_SwitchOnDlcButton.transform.parent.gameObject.SetActive(SwitchOnButtonActive.Value);
		}
	}

	protected virtual void CheckAvailableStateImpl(bool available)
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
	}
}
