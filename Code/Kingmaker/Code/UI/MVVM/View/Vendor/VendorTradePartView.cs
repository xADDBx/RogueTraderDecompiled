using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorTradePartView<TItemsFilter, TVendorSlot, TVendorTransitionWindow> : ViewBase<VendorTradePartVM> where TItemsFilter : ItemsFilterPCView where TVendorSlot : VendorLevelItemsBaseView where TVendorTransitionWindow : VendorTransitionWindowView
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_VendorName;

	[SerializeField]
	protected TextMeshProUGUI m_VendorHidenReputationName;

	[SerializeField]
	protected TextMeshProUGUI m_VendorHidenInfoText;

	[SerializeField]
	protected TextMeshProUGUI m_VendorHidenPFText;

	[SerializeField]
	protected Image m_VendorPortrait;

	[SerializeField]
	protected GameObject m_VendorNoPortraitEffect;

	[SerializeField]
	protected TextMeshProUGUI m_VendorNoPortraitNoDataText;

	[SerializeField]
	protected VendorRandomPhrases m_VendorPhrasesList;

	[SerializeField]
	protected ScrambledTMP m_VendorPhrase;

	[SerializeField]
	protected Image m_VendorReputationProgressToNextLevel;

	[SerializeField]
	protected TItemsFilter m_VendorItemsFilter;

	[SerializeField]
	protected TVendorSlot m_VendorSlotPrefab;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationValues;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationHeader;

	[SerializeField]
	protected TVendorTransitionWindow m_TransitionWindowPCView;

	[SerializeField]
	protected TextMeshProUGUI m_VendorReputationLevel;

	[SerializeField]
	protected TextMeshProUGUI FractionName;

	[SerializeField]
	public ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected Image m_PlayerPortrait;

	[SerializeField]
	protected TextMeshProUGUI m_PlayerName;

	[SerializeField]
	protected TextMeshProUGUI m_ProfitFactorText;

	[SerializeField]
	protected Image m_PlayerNoReputationPortrait;

	[SerializeField]
	protected TextMeshProUGUI m_PlayerNoReputationName;

	[SerializeField]
	protected TextMeshProUGUI m_ProfitNoReputationFactorText;

	[SerializeField]
	protected TextMeshProUGUI m_ProfitFactorValue;

	[SerializeField]
	protected OwlcatMultiButton m_ProfitFactorBackground;

	[SerializeField]
	protected GameObject m_NoItemsToSell;

	[SerializeField]
	protected TextMeshProUGUI m_NoItemsToSellText;

	public ReactiveCommand OnUpdateSlots = new ReactiveCommand();

	protected TooltipTemplateProfitFactor m_ProfitFactorTooltip;

	[SerializeField]
	protected CanvasGroup m_VendorInfoGroup;

	[SerializeField]
	protected CanvasGroup m_VendorHidenReputationGroup;

	[SerializeField]
	protected CanvasGroup m_VendorInfoGroupReputation;

	[Header("DiscountBlock")]
	[SerializeField]
	protected TextMeshProUGUI m_DiscountText;

	[SerializeField]
	protected TextMeshProUGUI m_DiscountValue;

	[SerializeField]
	protected GameObject m_DiscountBlock;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_VendorItemsFilter.Initialize();
		m_TransitionWindowPCView.Initialize();
		m_VendorNoPortraitNoDataText.text = UIStrings.Instance.QuesJournalTexts.NoData;
	}

	protected override void BindViewImplementation()
	{
		m_FadeAnimator.AppearAnimation();
		SetReputationAndPF(base.ViewModel.NeedHidePfAndReputation);
		if (base.ViewModel.NeedHidePfAndReputation)
		{
			m_VendorHidenInfoText.text = "\\\\-- > " + UIStrings.Instance.ExplorationTexts.ExploNotInteractable.Text + " < ---";
			m_VendorHidenPFText.text = UIStrings.Instance.QuesJournalTexts.NoData.Text;
			m_PlayerNoReputationPortrait.sprite = base.ViewModel.PlayerPortrait;
			m_PlayerNoReputationName.text = base.ViewModel.PlayerName;
			m_ProfitNoReputationFactorText.text = base.ViewModel.ProfitFactorText;
		}
		if (!base.ViewModel.NeedHidePfAndReputation && base.ViewModel.NeedHideReputationCompletely)
		{
			m_VendorInfoGroupReputation.gameObject.SetActive(value: false);
		}
		else
		{
			m_VendorInfoGroupReputation.gameObject.SetActive(value: true);
		}
		m_DiscountBlock.SetActive(base.ViewModel.HasDiscount);
		if (base.ViewModel.HasDiscount)
		{
			m_DiscountText.text = UIStrings.Instance.Vendor.Discount;
			m_DiscountValue.text = base.ViewModel.DiscountValue.ToString();
		}
		AddDisposable(base.ViewModel.OnSlotsUpdate.Subscribe(DrawEntities));
		AddDisposable(base.ViewModel.VendorName.Subscribe(delegate(string value)
		{
			m_VendorName.text = value;
		}));
		AddDisposable(base.ViewModel.VendorName.Subscribe(delegate(string value)
		{
			m_VendorHidenReputationName.text = value;
		}));
		AddDisposable(base.ViewModel.VendorSprite.Subscribe(SetVendorPortrait));
		AddDisposable(base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate
		{
			if (base.ViewModel.Difference != null)
			{
				m_VendorReputationProgressToNextLevel.fillAmount = base.ViewModel.Difference.Value / (float)base.ViewModel.Delta.Value;
			}
		}));
		SetVendorPhrase(helloWord: true);
		AddDisposable(base.ViewModel.VendorReputationLevel.Subscribe(delegate(int l)
		{
			m_VendorReputationLevel.text = l.ToString();
		}));
		AddDisposable(base.ViewModel.VendorReputationProgressToNextLevel.Subscribe(delegate(int? exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.Value ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : $"{base.ViewModel.VendorCurrentReputationProgress} / {exp.ToString()}");
		}));
		AddDisposable(base.ViewModel.VendorCurrentReputationProgress.Subscribe(delegate(float exp)
		{
			m_ReputationValues.text = (base.ViewModel.IsMaxLevel.Value ? ((string)UIStrings.Instance.CharacterSheet.MaxReputationLevel) : (exp + " / " + base.ViewModel.VendorReputationProgressToNextLevel.Value));
		}));
		AddDisposable(base.ViewModel.TransitionWindowVM.Subscribe(delegate(VendorTransitionWindowVM val)
		{
			m_TransitionWindowPCView.Bind(val);
		}));
		FractionName.text = base.ViewModel.VendorFractionName;
		m_ProfitFactorValue.text = Game.Instance.Player.ProfitFactor.Total.ToString();
		m_ReputationHeader.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		m_PlayerPortrait.sprite = base.ViewModel.PlayerPortrait;
		m_PlayerName.text = base.ViewModel.PlayerName;
		m_ProfitFactorText.text = base.ViewModel.ProfitFactorText;
		m_ProfitFactorTooltip = new TooltipTemplateProfitFactor(base.ViewModel.ProfitFactorVM);
		AddDisposable(m_ProfitFactorBackground.SetTooltip(m_ProfitFactorTooltip));
		if (m_NoItemsToSellText != null)
		{
			m_NoItemsToSellText.text = UIStrings.Instance.Tooltips.NoItemsAvailableToSelect;
		}
		AddDisposable(base.ViewModel.VendorHasItemsToSell.Subscribe(SetEmptyCargoText));
		DrawEntities();
	}

	protected void SetEmptyCargoText(bool value)
	{
		if (m_NoItemsToSell != null)
		{
			m_NoItemsToSell.SetActive(!value);
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EnableSlots, m_VendorSlotPrefab);
		m_ScrollRect.ScrollToTop();
		OnUpdateSlots?.Execute();
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
		base.gameObject.SetActive(value: false);
	}

	protected void SetVendorPortrait(Sprite portrait)
	{
		m_VendorPortrait.gameObject.SetActive(portrait != null);
		m_VendorNoPortraitEffect.SetActive(portrait == null);
		if (portrait != null)
		{
			m_VendorPortrait.sprite = portrait;
		}
	}

	protected void SetVendorPhrase(bool helloWord)
	{
		if (helloWord)
		{
			m_VendorPhrase.SetText(string.Empty, m_VendorPhrasesList.TakePhrase(base.ViewModel.VendorFaction.Value, Game.Instance.Vendor.VendorEntity as BaseUnitEntity));
		}
		else
		{
			m_VendorPhrase.SetText(string.Empty, m_VendorPhrasesList.TakeFinishDealPhrase(base.ViewModel.VendorFaction.Value, Game.Instance.Vendor.VendorEntity as BaseUnitEntity));
		}
	}

	private void SetReputationAndPF(bool value)
	{
		m_VendorInfoGroup.gameObject.SetActive(!value);
		m_VendorHidenReputationGroup.gameObject.SetActive(value);
	}
}
