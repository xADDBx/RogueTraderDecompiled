using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.DLC;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;

public class DlcManagerDlcEntityBaseView : SelectionGroupEntityView<DlcManagerDlcEntityVM>, IWidgetView
{
	[Serializable]
	private class SecondLabelSettings
	{
		public DlcTypeEnum Type;

		public Color BackgroundColor;

		public Color TextColor;
	}

	[Serializable]
	private class PurchaseStateSettings
	{
		public BlueprintDlc.DlcPurchaseState State;

		public Color TextColor;

		public Sprite AdditionalMarkSprite;

		public Color BackgroundGradientsColor;
	}

	[Header("MainContent")]
	[SerializeField]
	private TextMeshProUGUI m_DlcTitle;

	[SerializeField]
	private Image m_DlcImage;

	[SerializeField]
	private TextMeshProUGUI m_NewMark;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[SerializeField]
	public List<Image> BackgroundGradientsItems;

	[Header("SecondLabelContent")]
	[SerializeField]
	private TextMeshProUGUI m_SecondLabelTitle;

	[SerializeField]
	private Image m_SecondLabelBackground;

	[SerializeField]
	private List<SecondLabelSettings> m_TypeSettings;

	[SerializeField]
	private TextMeshProUGUI m_StoryCompanyName;

	[Header("PurchaseStateContent")]
	[SerializeField]
	private TextMeshProUGUI m_PurchaseStateLabel;

	[SerializeField]
	private Image m_AdditionalMarkImage;

	[SerializeField]
	private List<PurchaseStateSettings> m_PurchaseStateSettings;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetMainContent();
		SetSecondLabelContent();
		SetPurchaseStateContent();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		WidgetFactory.DisposeWidget(this);
	}

	private void SetMainContent()
	{
		m_DlcTitle.text = base.ViewModel.Title;
		PurchaseStateSettings currentState = m_PurchaseStateSettings.FirstOrDefault((PurchaseStateSettings ps) => ps.State == base.ViewModel.BlueprintDlc.GetPurchaseState());
		if (currentState != null)
		{
			BackgroundGradientsItems.ForEach(delegate(Image i)
			{
				i.color = currentState.BackgroundGradientsColor;
			});
		}
		m_DlcImage.sprite = base.ViewModel.Art;
		m_NewMark.text = UIStrings.Instance.QuestNotificationTexts.New.Text + "!";
		AddDisposable(base.ViewModel.SawThisDlc.Subscribe(delegate(bool value)
		{
			m_NewMark.gameObject.SetActive(!value);
		}));
		UpdateGrayScale();
	}

	private void SetSecondLabelContent()
	{
		m_SecondLabelTitle.text = UIStrings.Instance.DlcManager.GetDlcTypeLabel(base.ViewModel.DlcType);
		SecondLabelSettings secondLabelSettings = m_TypeSettings.FirstOrDefault((SecondLabelSettings ts) => ts.Type == base.ViewModel.DlcType);
		if (secondLabelSettings == null)
		{
			return;
		}
		m_SecondLabelTitle.color = secondLabelSettings.TextColor;
		m_SecondLabelBackground.color = secondLabelSettings.BackgroundColor;
		if (base.ViewModel.BlueprintDlc.ParentDlc == null)
		{
			BlueprintDlcRewardCampaignAdditionalContent blueprintDlcRewardCampaignAdditionalContent = base.ViewModel.BlueprintDlc.Rewards.FirstOrDefault((IBlueprintDlcReward r) => r is BlueprintDlcRewardCampaignAdditionalContent) as BlueprintDlcRewardCampaignAdditionalContent;
			if (blueprintDlcRewardCampaignAdditionalContent?.Campaign == null)
			{
				m_StoryCompanyName.gameObject.SetActive(value: false);
				return;
			}
			string text = blueprintDlcRewardCampaignAdditionalContent.Campaign?.Title;
			m_StoryCompanyName.gameObject.SetActive(secondLabelSettings.Type == DlcTypeEnum.AdditionalContentDlc && !string.IsNullOrWhiteSpace(text));
			m_StoryCompanyName.text = "*" + UIStrings.Instance.DlcManager.StoryCompanyIs.Text + " " + text;
		}
		else
		{
			m_StoryCompanyName.gameObject.SetActive(secondLabelSettings.Type == DlcTypeEnum.AdditionalContentDlc);
			m_StoryCompanyName.text = "*" + UIStrings.Instance.DlcManager.StoryCompanyIs.Text + " " + base.ViewModel.BlueprintDlc.ParentDlc.GetDlcName();
		}
	}

	private void SetPurchaseStateContent()
	{
		PurchaseStateSettings purchaseStateSettings = m_PurchaseStateSettings.FirstOrDefault((PurchaseStateSettings ps) => ps.State == base.ViewModel.BlueprintDlc.GetPurchaseState());
		if (purchaseStateSettings != null)
		{
			m_PurchaseStateLabel.text = UIStrings.Instance.DlcManager.GetDlcPurchaseStateLabel(purchaseStateSettings.State);
			m_PurchaseStateLabel.color = purchaseStateSettings.TextColor;
			m_AdditionalMarkImage.gameObject.SetActive(purchaseStateSettings.AdditionalMarkSprite != null);
			if (purchaseStateSettings.AdditionalMarkSprite != null)
			{
				m_AdditionalMarkImage.sprite = purchaseStateSettings.AdditionalMarkSprite;
			}
		}
	}

	public void UpdateGrayScale()
	{
		if (!(m_GrayScale == null))
		{
			bool flag = base.ViewModel.BlueprintDlc.GetPurchaseState() == BlueprintDlc.DlcPurchaseState.ComingSoon;
			m_GrayScale.EffectAmount = ((!flag) ? 0f : 0.8f);
			m_GrayScale.Alpha = ((!flag) ? 1f : 0.5f);
		}
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		if (value)
		{
			base.ViewModel.SelectMe();
		}
		OnChangeSelectedStateImpl(value);
	}

	protected virtual void OnChangeSelectedStateImpl(bool value)
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DlcManagerDlcEntityVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DlcManagerDlcEntityVM;
	}
}
