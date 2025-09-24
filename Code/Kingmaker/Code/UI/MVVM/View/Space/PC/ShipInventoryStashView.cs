using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Loot;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class ShipInventoryStashView : ViewBase<ShipInventoryStashVM>
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private TextMeshProUGUI m_ScrapCounter;

	[SerializeField]
	private TextMeshProUGUI m_ScrapName;

	[SerializeField]
	protected OwlcatMultiButton m_CoinsContainer;

	[SerializeField]
	protected ItemSlotsGroupView m_ItemSlotsGroup;

	[SerializeField]
	protected InsertableLootSlotsGroupView m_InsertableSlotsGroup;

	[SerializeField]
	protected ShipItemsFilterPCView m_ItemsFilter;

	[SerializeField]
	private InventorySlotView m_InventorySlotPrefab;

	[SerializeField]
	private InsertableLootSlotView m_InsertableSlotPrefab;

	[SerializeField]
	private CanvasGroup m_ScrapMark;

	[SerializeField]
	private OwlcatMultiButton m_SortButton;

	private string m_ScrapText;

	public void Initialize()
	{
		m_ItemSlotsGroup.Initialize(m_InventorySlotPrefab);
		m_InsertableSlotsGroup.Or(null)?.Initialize(m_InsertableSlotPrefab);
		m_ItemsFilter.Initialize();
		Hide();
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_ScrapText = UIStrings.Instance.ShipCustomization.Scrap.Text;
		AddDisposable(base.ViewModel.Scrap.Subscribe(delegate(long value)
		{
			UpdateScrap(value);
		}));
		AddDisposable(m_CoinsContainer.SetGlossaryTooltip("ScrapSpace", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		AddDisposable(UniRxExtensionMethods.Subscribe(m_SortButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ItemSlotsGroup.SortItems();
		}));
		m_ItemSlotsGroup.Bind(base.ViewModel.ItemSlotsGroup);
		m_InsertableSlotsGroup.Or(null)?.Bind(base.ViewModel.InsertableSlotsGroup);
		m_ItemsFilter.Bind(base.ViewModel.ItemsFilter);
	}

	private void UpdateScrap(long value)
	{
		m_ScrapCounter.text = value.ToString();
		m_ScrapName.text = m_ScrapText;
		UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
		m_ScrapMark.gameObject.SetActive(value: true);
		m_ScrapMark.alpha = 1f;
		m_ScrapMark.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_Background.Or(null)?.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_Background.Or(null)?.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	public void CollectionChanged()
	{
		base.ViewModel.CollectionChanged();
	}
}
