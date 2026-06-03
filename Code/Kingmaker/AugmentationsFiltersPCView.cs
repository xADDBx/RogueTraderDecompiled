using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker;

public class AugmentationsFiltersPCView : ViewBase<ItemsFilterVM>
{
	[SerializeField]
	private GameObject m_Lens;

	[SerializeField]
	private float FilterSwitchAnimationDuration = 0.55f;

	[SerializeField]
	private OwlcatToggleGroup m_FiltersToggleGroup;

	[FormerlySerializedAs("m_None")]
	[SerializeField]
	protected OwlcatToggle m_AugmentationsAll;

	[FormerlySerializedAs("m_Augmentations")]
	[SerializeField]
	protected OwlcatToggle m_AugmentationsArms;

	[SerializeField]
	protected OwlcatToggle m_AugmentationsEyes;

	[SerializeField]
	protected OwlcatToggle m_AugmentationsLegs;

	[SerializeField]
	protected OwlcatToggle m_AugmentationsTorso;

	[SerializeField]
	protected OwlcatToggle m_AugmentationsSystems;

	[FormerlySerializedAs("m_AugmentationsMechanicus")]
	[SerializeField]
	protected OwlcatToggle m_AugmentationsMisc;

	[SerializeField]
	public OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	private GameObject m_SorterObject;

	[SerializeField]
	private VirtualListComponent m_VirtualList;

	[Header("Search Part")]
	[SerializeField]
	public ItemsFilterSearchBaseView m_SearchView;

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

	private const float LensThreshold = 0.0001f;

	protected Dictionary<ItemsFilterType, OwlcatToggle> m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>();

	[FormerlySerializedAs("m_ShipNoFilterPosition")]
	[SerializeField]
	private float m_AugmentationsNoFilterPosition = -155f;

	[SerializeField]
	private float m_ShipAugmentationsPosition = -85f;

	[Header("Filters")]
	[SerializeField]
	protected List<ItemsFilterType> m_SortedFiltersList = new List<ItemsFilterType>
	{
		ItemsFilterType.AugmentationsAll,
		ItemsFilterType.AugmentationsArms,
		ItemsFilterType.AugmentationsEyes,
		ItemsFilterType.AugmentationsLegs,
		ItemsFilterType.AugmentationsSystems,
		ItemsFilterType.AugmentationsTorso,
		ItemsFilterType.AugmentationsMisc
	};

	public bool m_VisibleSearchBar;

	private ItemsFilterType m_FirstFilter;

	private ItemsFilterType m_LastFilter;

	private AccessibilityTextHelper m_TextHelper;

	public virtual void Initialize()
	{
		Hide();
		m_FiltersMap = new Dictionary<ItemsFilterType, OwlcatToggle>
		{
			{
				ItemsFilterType.AugmentationsAll,
				m_AugmentationsAll
			},
			{
				ItemsFilterType.AugmentationsArms,
				m_AugmentationsArms
			},
			{
				ItemsFilterType.AugmentationsEyes,
				m_AugmentationsEyes
			},
			{
				ItemsFilterType.AugmentationsLegs,
				m_AugmentationsLegs
			},
			{
				ItemsFilterType.AugmentationsSystems,
				m_AugmentationsSystems
			},
			{
				ItemsFilterType.AugmentationsTorso,
				m_AugmentationsTorso
			},
			{
				ItemsFilterType.AugmentationsMisc,
				m_AugmentationsMisc
			}
		};
		m_FirstFilter = m_SortedFiltersList.FirstOrDefault();
		m_LastFilter = m_SortedFiltersList.LastOrDefault();
		m_SearchView.Or(null)?.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_ToggleLabel);
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
			m_AugmentationsAll.Set(value: true);
		}
		SetupToggleGroup();
		m_TextHelper.UpdateTextSize();
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

	private void HandleFilterToggle(OwlcatToggle activeToggle)
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
				ScrollToTop();
				float num = m_LensStartPosition + (float)m_SortedFiltersList.IndexOf(itemsFilterType2) * m_LensOffsetDelta;
				if (Math.Abs(m_Lens.transform.localPosition.x - num) > 0.0001f)
				{
					AddDisposable(UIUtility.CreateMoveXLensPosition(m_Lens.transform, num, FilterSwitchAnimationDuration));
				}
				break;
			}
		}
	}

	private void SetHints()
	{
		AddDisposable(m_AugmentationsAll.SetHint(UIStrings.Instance.UIAugmentations.FilterAll));
		AddDisposable(m_AugmentationsArms.SetHint(UIStrings.Instance.UIAugmentations.FilterArms));
		AddDisposable(m_AugmentationsEyes.SetHint(UIStrings.Instance.UIAugmentations.FilterEyes));
		AddDisposable(m_AugmentationsLegs.SetHint(UIStrings.Instance.UIAugmentations.FilterLegs));
		AddDisposable(m_AugmentationsSystems.SetHint(UIStrings.Instance.UIAugmentations.FilterSystems));
		AddDisposable(m_AugmentationsTorso.SetHint(UIStrings.Instance.UIAugmentations.FilterTorso));
		AddDisposable(m_AugmentationsMisc.SetHint(UIStrings.Instance.UIAugmentations.FilterForgeworld));
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
		m_TextHelper.Dispose();
	}
}
