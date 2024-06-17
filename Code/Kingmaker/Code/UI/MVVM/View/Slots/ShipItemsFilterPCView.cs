using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ShipItemsFilterPCView : ViewBase<ItemsFilterVM>
{
	protected Dictionary<OwlcatToggle, (ItemsFilterType, float)> m_Filters;

	[SerializeField]
	private GameObject m_Lens;

	[SerializeField]
	private float FilterSwitchAnimationDuration = 0.55f;

	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	protected OwlcatToggle m_None;

	[SerializeField]
	protected OwlcatToggle m_Weapon;

	[SerializeField]
	protected OwlcatToggle m_Other;

	[SerializeField]
	protected OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	private GameObject m_SorterObject;

	[SerializeField]
	private VirtualListComponent m_VirtualList;

	[Header("Search Part")]
	[SerializeField]
	protected ItemsFilterSearchBaseView m_SearchView;

	[SerializeField]
	private float m_ShipNoFilterPosition = -155f;

	[SerializeField]
	private float m_ShipWeaponPosition = -85f;

	[SerializeField]
	private float ShipOtherPosition;

	protected bool m_VisibleSearchBar;

	public virtual void Initialize()
	{
		Hide();
		m_Filters = new Dictionary<OwlcatToggle, (ItemsFilterType, float)>
		{
			{
				m_None,
				(ItemsFilterType.ShipNoFilter, m_ShipNoFilterPosition)
			},
			{
				m_Weapon,
				(ItemsFilterType.ShipWeapon, m_ShipWeaponPosition)
			},
			{
				m_Other,
				(ItemsFilterType.ShipOther, ShipOtherPosition)
			}
		};
		m_SearchView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(m_FiltersToggleGroup.ActiveToggle.Subscribe(HandleFilterToggle));
		if (m_SorterDropdown != null)
		{
			AddDisposable(m_SorterDropdown.Index.Subscribe(delegate
			{
				OnSorterDropdownValueChanged();
			}));
		}
		SetHints();
		m_SearchView.Or(null)?.Bind(base.ViewModel.ItemsFilterSearchVM);
		if (!m_FiltersToggleGroup.AnyTogglesOn())
		{
			m_None.Set(value: true);
		}
	}

	private void HandleFilterToggle(OwlcatToggle activeToggle)
	{
		if (!(activeToggle == null) && m_Filters.TryGetValue(activeToggle, out var value))
		{
			base.ViewModel.SetCurrentFilter(value.Item1);
			ScrollToTop();
			if (m_Lens.transform.localPosition.x != value.Item2)
			{
				UIUtility.MoveXLensPosition(m_Lens.transform, value.Item2, FilterSwitchAnimationDuration);
			}
		}
	}

	private void SetHints()
	{
		AddDisposable(m_None.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAll));
		AddDisposable(m_Weapon.SetHint(UIStrings.Instance.InventoryScreen.FilterTextWeapon));
		AddDisposable(m_Other.SetHint(UIStrings.Instance.InventoryScreen.FilterTextOther));
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_SorterObject.Or(null)?.SetActive(value: true);
		m_SorterDropdown.Bind(base.ViewModel.SorterDropdownVM);
		OnSorterDropdownValueChanged();
	}

	public void OnSorterDropdownValueChanged()
	{
		int value = m_SorterDropdown.Index.Value;
		ItemsSorterType currentSorter = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType)))[value];
		base.ViewModel.SetCurrentSorter(currentSorter);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		m_SorterObject.Or(null)?.SetActive(value: false);
	}

	private void ScrollToTop()
	{
		m_VirtualList.Or(null)?.ScrollController?.ForceScrollToTop();
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
		Hide();
	}
}
