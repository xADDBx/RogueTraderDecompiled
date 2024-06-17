using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationPartView<TInventoryCargo, TVendorReputationForItem> : ViewBase<VendorReputationPartVM> where TInventoryCargo : InventoryCargoView where TVendorReputationForItem : VendorReputationForItemWindowView
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TInventoryCargo m_InventoryCargoPCView;

	[SerializeField]
	protected TextMeshProUGUI m_DemandCargo;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationValues;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationHeader;

	[SerializeField]
	protected TextMeshProUGUI m_VendorReputationLevelInCircle;

	[SerializeField]
	protected TextMeshProUGUI FractionName;

	[SerializeField]
	protected TVendorReputationForItem m_ReputationForItemWindowPCView;

	[SerializeField]
	protected Image m_VendorReputationProgressToNextLevel;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private TextMeshProUGUI m_ExchangeValue;

	[SerializeField]
	protected OwlcatToggle m_ShowUnrelevantToggle;

	[SerializeField]
	private TextMeshProUGUI m_ShowUnrelevantLabel;

	[SerializeField]
	protected OwlcatButton SellButton;

	[SerializeField]
	protected TextMeshProUGUI m_SellButtonText;

	[SerializeField]
	protected CanvasGroup m_VendorInfoGroup;

	[SerializeField]
	protected CanvasGroup m_VendorHidenReputationGroup;

	[SerializeField]
	protected TextMeshProUGUI m_VendorHidenInfoText;

	private IDisposable m_ToggleDisposable;

	public BoolReactiveProperty HasVisibleCargo => base.ViewModel.InventoryCargoVM.HasVisibleCargo;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_InventoryCargoPCView.Initialize();
		m_ReputationForItemWindowPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
		SetReputation(base.ViewModel.NeedHidePfAndReputation);
		if (base.ViewModel.NeedHidePfAndReputation)
		{
			m_VendorHidenInfoText.text = UIStrings.Instance.QuesJournalTexts.NoData.Text;
		}
		m_InventoryCargoPCView.Bind(base.ViewModel.InventoryCargoVM);
		m_ReputationForItemWindowPCView.Bind(base.ViewModel.VendorReputationForItemWindow);
		m_SelectorView.Bind(base.ViewModel.Selector);
		AddDisposable(base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate
		{
			m_VendorReputationProgressToNextLevel.fillAmount = base.ViewModel.Difference.Value / (float)base.ViewModel.Delta.Value;
		}));
		AddDisposable(base.ViewModel.VendorReputationProgressToNextLevel.Subscribe(delegate(int? exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.Value ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : $"{base.ViewModel.VendorCurrentReputationProgress} / {exp.ToString()}");
		}));
		AddDisposable(base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate(float exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.Value ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : (exp + " / " + base.ViewModel.VendorReputationProgressToNextLevel.Value));
		}));
		AddDisposable(base.ViewModel.VendorReputationLevel.Subscribe(delegate(int l)
		{
			m_VendorReputationLevelInCircle.text = l.ToString();
		}));
		AddDisposable(base.ViewModel.ExchangeValue.Subscribe(delegate(int val)
		{
			m_ExchangeValue.text = val.ToString();
		}));
		m_DemandCargo.text = UIStrings.Instance.Vendor.DemandCargo;
		m_ReputationHeader.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		FractionName.text = base.ViewModel.VendorFractionName;
		m_InventoryCargoPCView.m_ListContentFadeAnimator.AppearAnimation();
		m_ShowUnrelevantLabel.text = UIStrings.Instance.Vendor.HideUnrelevant;
		m_SellButtonText.text = UIStrings.Instance.Vendor.Exchange;
		AddDisposable(base.ViewModel.CanSellCargo.Subscribe(delegate(bool val)
		{
			SellButton.Interactable = val;
		}));
		m_ShowUnrelevantToggle.Set(base.ViewModel.InventoryCargoVM.HideUnrelevant.Value);
		m_ToggleDisposable = m_ShowUnrelevantToggle.IsOn.Skip(1).Subscribe(delegate
		{
			base.ViewModel.HideUnrelevant();
		});
	}

	private void SetReputation(bool value)
	{
		m_VendorInfoGroup.gameObject.SetActive(!value);
		m_VendorHidenReputationGroup.gameObject.SetActive(value);
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
		m_ToggleDisposable?.Dispose();
		m_ToggleDisposable = null;
		base.gameObject.SetActive(value: false);
	}
}
