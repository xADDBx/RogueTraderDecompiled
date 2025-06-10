using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemsFilterPCView : ViewBase<ItemsFilterVM>
{
	[Header("Toggles")]
	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[SerializeField]
	private OwlcatToggle m_None;

	[SerializeField]
	private OwlcatToggle m_Weapon;

	[SerializeField]
	private OwlcatToggle m_Armor;

	[SerializeField]
	private OwlcatToggle m_Accessories;

	[SerializeField]
	private OwlcatToggle m_Usable;

	[SerializeField]
	private OwlcatToggle m_Notable;

	[SerializeField]
	private OwlcatToggle m_ShipItems;

	[SerializeField]
	private OwlcatToggle m_Other;

	[Header("Components")]
	[SerializeField]
	private GameObject m_Lens;

	[SerializeField]
	protected OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	private GameObject m_SorterObject;

	[SerializeField]
	private VirtualListComponent m_VirtualList;

	[Header("Search Part")]
	[SerializeField]
	protected ItemsFilterSearchBaseView m_SearchView;

	[Header("Filters")]
	[SerializeField]
	protected List<ItemsFilterType> m_SortedFiltersList = new List<ItemsFilterType>
	{
		ItemsFilterType.NoFilter,
		ItemsFilterType.Weapon,
		ItemsFilterType.Armor,
		ItemsFilterType.Accessories,
		ItemsFilterType.Usable,
		ItemsFilterType.Notable,
		ItemsFilterType.NonUsable,
		ItemsFilterType.ShipNoFilter
	};

	[Header("AvailableItems")]
	[SerializeField]
	protected bool m_ShowToggle;

	[SerializeField]
	private GameObject m_ToggleParent;

	[SerializeField]
	protected OwlcatToggle m_Toggle;

	[SerializeField]
	private TextMeshProUGUI m_ToggleLabel;

	[Header("Values")]
	[SerializeField]
	private float m_LensStartPosition = -185f;

	[SerializeField]
	private float m_LensOffsetDelta = 54.5f;

	[SerializeField]
	private float FilterSwitchAnimationDuration = 0.55f;

	private const float LensThreshold = 0.0001f;

	private Dictionary<ItemsFilterType, OwlcatToggle> m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>();

	private ItemsFilterType m_FirstFilter;

	private ItemsFilterType m_LastFilter;

	private AccessibilityTextHelper m_TextHelper;

	public virtual void Initialize()
	{
		Hide();
		m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>
		{
			{
				ItemsFilterType.NoFilter,
				m_None
			},
			{
				ItemsFilterType.Weapon,
				m_Weapon
			},
			{
				ItemsFilterType.Armor,
				m_Armor
			},
			{
				ItemsFilterType.Accessories,
				m_Accessories
			},
			{
				ItemsFilterType.Usable,
				m_Usable
			},
			{
				ItemsFilterType.Notable,
				m_Notable
			},
			{
				ItemsFilterType.NonUsable,
				m_Other
			},
			{
				ItemsFilterType.ShipNoFilter,
				m_ShipItems
			}
		};
		m_SearchView.Or(null)?.Initialize();
		m_FirstFilter = m_SortedFiltersList.FirstOrDefault();
		m_LastFilter = m_SortedFiltersList.LastOrDefault();
		m_TextHelper = new AccessibilityTextHelper(m_ToggleLabel);
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(m_FiltersToggleGroup.ActiveToggle.Subscribe(delegate(OwlcatToggle val)
		{
			HandleFilterToggle(val);
		}));
		AddDisposable(base.ViewModel.OnFilterReset.Subscribe(delegate
		{
			HandleFilterToggle(m_None, fromBuying: true);
		}));
		if (m_SorterDropdown != null)
		{
			AddDisposable(m_SorterDropdown.Index.Subscribe(delegate
			{
				OnSorterDropdownValueChanged();
			}));
		}
		SetHints();
		m_SearchView.Or(null)?.Bind(base.ViewModel.ItemsFilterSearchVM);
		if (base.ViewModel.CurrentFilter == null || !m_FiltersMap.TryGetValue(base.ViewModel.CurrentFilter.Value, out var value))
		{
			value = m_None;
		}
		value.Set(value: true);
		AddDisposable(base.ViewModel.CurrentFilter.Subscribe(OnCurrentFilterChanged));
		AddDisposable(base.ViewModel.CurrentSorter.Subscribe(OnCurrentSorterChanged));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, delegate
		{
			OnPrevious(default(InputActionEventData));
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, delegate
		{
			OnNext(default(InputActionEventData));
		}));
		SetupToggleGroup();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
		Hide();
		m_TextHelper.Dispose();
	}

	private void SetHints()
	{
		AddDisposable(m_None.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAll));
		AddDisposable(m_Weapon.SetHint(UIStrings.Instance.InventoryScreen.FilterTextWeapon));
		AddDisposable(m_Armor.SetHint(UIStrings.Instance.InventoryScreen.FilterTextArmor));
		AddDisposable(m_Accessories.SetHint(UIStrings.Instance.InventoryScreen.FilterTextAcessories));
		AddDisposable(m_Usable.SetHint(UIStrings.Instance.InventoryScreen.FilterTextUsable));
		AddDisposable(m_Notable.SetHint(UIStrings.Instance.InventoryScreen.FilterTextNotable));
		AddDisposable(m_ShipItems.SetHint(UIStrings.Instance.InventoryScreen.FilterTextShipItem));
		AddDisposable(m_Other.SetHint(UIStrings.Instance.InventoryScreen.FilterTextOther));
	}

	private void SetupToggleGroup()
	{
		m_ToggleParent.Or(null)?.SetActive(m_ShowToggle);
		if ((bool)m_ToggleLabel)
		{
			m_ToggleLabel.text = UIStrings.Instance.InventoryScreen.ShowUnavailableItems;
		}
		if ((bool)m_Toggle)
		{
			m_Toggle.Set(!base.ViewModel.ShowUnavailable.Value);
			AddDisposable(m_Toggle.IsOn.Skip(1).Subscribe(delegate(bool value)
			{
				base.ViewModel.ShowUnavailable.Value = !value;
			}));
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_SorterObject.Or(null)?.SetActive(value: true);
		m_SorterDropdown.Bind(base.ViewModel.SorterDropdownVM);
		OnSorterDropdownValueChanged();
	}

	public void HandleFilterToggle(OwlcatToggle activeToggle, bool fromBuying = false)
	{
		if (activeToggle == null)
		{
			return;
		}
		foreach (var (itemsFilterType2, owlcatToggle2) in m_FiltersMap)
		{
			if (!(owlcatToggle2 != activeToggle))
			{
				base.ViewModel.SetCurrentFilter(itemsFilterType2);
				if (!fromBuying)
				{
					ScrollToTop();
				}
				float num = m_LensStartPosition + (float)m_SortedFiltersList.IndexOf(itemsFilterType2) * m_LensOffsetDelta;
				if (Math.Abs(m_Lens.transform.localPosition.x - num) > 0.0001f)
				{
					AddDisposable(UIUtility.CreateMoveXLensPosition(m_Lens.transform, num, FilterSwitchAnimationDuration));
				}
				break;
			}
		}
	}

	public void HandleFilterToggle(ItemsFilterType type, bool fromBuying = false)
	{
		base.ViewModel.SetCurrentFilter(type);
		float num = m_LensStartPosition + (float)m_SortedFiltersList.IndexOf(type) * m_LensOffsetDelta;
		if (Math.Abs(m_Lens.transform.localPosition.x - num) > 0.0001f)
		{
			UIUtility.CreateMoveXLensPosition(m_Lens.transform, num, FilterSwitchAnimationDuration);
		}
	}

	private void OnSorterDropdownValueChanged()
	{
		int value = m_SorterDropdown.Index.Value;
		ItemsSorterType currentSorter = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType)))[value];
		base.ViewModel.SetCurrentSorter(currentSorter);
	}

	private void OnCurrentFilterChanged(ItemsFilterType filterType)
	{
		OwlcatToggle owlcatToggle = m_FiltersMap[filterType];
		if (!owlcatToggle.IsOn.Value)
		{
			owlcatToggle.Set(value: true);
		}
	}

	private void OnCurrentSorterChanged(ItemsSorterType sorterType)
	{
		int num = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType))).IndexOf(sorterType);
		if (num != m_SorterDropdown.Index.Value)
		{
			m_SorterDropdown.SetIndex(num);
		}
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

	public void OnPrevious(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == m_FirstFilter)
		{
			base.ViewModel.SetCurrentFilter(m_LastFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.Value) - 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}

	public void OnNext(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == m_LastFilter)
		{
			base.ViewModel.SetCurrentFilter(m_FirstFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.Value) + 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}
}
