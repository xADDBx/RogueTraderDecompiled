using Kingmaker.Code.UI.MVVM.View.Loot;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public abstract class InventoryStashView : ViewBase<InventoryStashVM>, IVendorBuyHandler, ISubscriber
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private TextMeshProUGUI m_CoinsCounter;

	[SerializeField]
	protected ItemSlotsGroupView m_ItemSlotsGroup;

	[SerializeField]
	protected InsertableLootSlotsGroupView m_InsertableSlotsGroup;

	[SerializeField]
	protected ItemsFilterPCView m_ItemsFilter;

	[SerializeField]
	private InventorySlotView m_InventorySlotPrefab;

	[SerializeField]
	private InsertableLootSlotView m_InsertableSlotPrefab;

	[SerializeField]
	private FadeAnimator m_RightSlots;

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
		AddDisposable(base.ViewModel.Money.Subscribe(delegate(long value)
		{
			m_CoinsCounter.text = value.ToString();
		}));
		m_ItemSlotsGroup.Bind(base.ViewModel.ItemSlotsGroup);
		m_InsertableSlotsGroup.Or(null)?.Bind(base.ViewModel.InsertableSlotsGroup);
		m_ItemsFilter.Bind(base.ViewModel.ItemsFilter);
		AddDisposable(EventBus.Subscribe(this));
		if (base.ViewModel.Unit == null)
		{
			return;
		}
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_RightSlots.PlayAnimation(value: true);
			if (u.IsPet)
			{
				m_RightSlots.PlayAnimation(value: false);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_Background.Or(null)?.SetActive(value: true);
		base.ViewModel.ItemSlotsGroup?.SortItems();
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_Background.Or(null)?.SetActive(value: false);
	}

	public void TryScrollToObject(ItemEntity element)
	{
		if (base.ViewModel.ItemSlotsGroup.ValidItems.FirstOrDefault((ItemEntity x) => x?.Blueprint == element.Blueprint) != null)
		{
			base.ViewModel.ResetFilter();
			DelayedInvoker.InvokeInFrames(delegate
			{
				ScrollToElement(element);
			}, 1);
		}
	}

	public void ScrollToElement(ItemEntity element)
	{
		ItemSlotVM visibleCollectionElement = base.ViewModel.ItemSlotsGroup.VisibleCollection.FirstOrDefault((ItemSlotVM x) => x.ItemEntity?.Blueprint == element.Blueprint);
		if (visibleCollectionElement != null)
		{
			m_ItemSlotsGroup.ForceScrollToElement(visibleCollectionElement);
			DelayedInvoker.InvokeInFrames(delegate
			{
				visibleCollectionElement.Blink();
			}, 1);
		}
	}

	public void CollectionChanged()
	{
		base.ViewModel.CollectionChanged();
	}

	public void HandleBuyItem(ItemEntity buyingItem)
	{
		TryScrollToObject(buyingItem);
	}

	public void SetFilter(ItemsFilterType type)
	{
		m_ItemsFilter.HandleFilterToggle(type);
	}
}
