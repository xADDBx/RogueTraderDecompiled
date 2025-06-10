using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class InventoryCargoView : ViewBase<InventoryCargoVM>
{
	[Header("Slots")]
	[SerializeField]
	protected VirtualListGridVertical m_VirtualList;

	[SerializeField]
	protected VirtualListElementViewBase<CargoSlotVM> m_SlotPrefab;

	[Header("Common")]
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected CargoDropZonePCView m_DropZonePCView;

	[SerializeField]
	public CargoDetailedZoneBaseView m_CargoZoneView;

	[SerializeField]
	public FadeAnimator m_ListContentFadeAnimator;

	[SerializeField]
	protected GameObject m_EmptyCargo;

	[SerializeField]
	protected TextMeshProUGUI m_EmptyCargoText;

	[SerializeField]
	protected TextMeshProUGUI m_CargoButtonText;

	[SerializeField]
	protected TextMeshProUGUI m_ListButtonText;

	[SerializeField]
	protected OwlcatDropdown m_SorterDropdown;

	[SerializeField]
	protected GameObject m_SorterBlock;

	[SerializeField]
	private float m_ScrollToCargoDelay = 0.2f;

	[SerializeField]
	protected GameObject LockedCargoBlock;

	[SerializeField]
	protected TextMeshProUGUI LockedCargoText;

	public readonly ReactiveCommand OnCargoViewChange = new ReactiveCommand();

	public readonly ReactiveCommand OnDetailedCargoShown = new ReactiveCommand();

	private IDisposable m_ScrollToDisposable;

	public CargoDetailedZoneBaseView CargoZoneView => m_CargoZoneView;

	public BoolReactiveProperty HasVisibleCargo => base.ViewModel.HasVisibleCargo;

	public BoolReactiveProperty IsCargoDetailed => base.ViewModel.IsCargoDetailedZone;

	public bool IsCargoLocked => base.ViewModel?.IsCargoLocked ?? false;

	public void Initialize()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<CargoSlotVM>(m_SlotPrefab));
		m_ListContentFadeAnimator.Initialize();
		if ((bool)m_CargoZoneView)
		{
			m_CargoZoneView.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		LockedCargoBlock.SetActive(IsCargoLocked);
		if (m_CargoButtonText != null && m_ListButtonText != null)
		{
			if ((bool)m_CargoButtonText)
			{
				m_CargoButtonText.text = UIStrings.Instance.CargoTexts.CargoList;
				m_ListButtonText.text = UIStrings.Instance.CargoTexts.Cargo;
				m_CargoButtonText.gameObject.SetActive(value: true);
				m_ListButtonText.gameObject.SetActive(value: true);
			}
			else
			{
				m_CargoButtonText.gameObject.SetActive(value: false);
				m_ListButtonText.gameObject.SetActive(value: false);
			}
		}
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.CargoSlots));
		if (m_DropZonePCView != null)
		{
			AddDisposable(base.ViewModel.CargoDropZoneVM.Subscribe(m_DropZonePCView.Bind));
			if (base.ViewModel.IsCargoLocked)
			{
				AddDisposable(m_DropZonePCView.HasItem.Subscribe(delegate(bool value)
				{
					LockedCargoBlock.SetActive(!value);
				}));
				LockedCargoText.text = (Game.Instance.Player.ServiceWindowsBlocked ? UIStrings.Instance.ExplorationTexts.ExploNotInteractable : UIStrings.Instance.LootWindow.LootLockedState);
			}
		}
		if (m_EmptyCargoText != null)
		{
			AddDisposable(base.ViewModel.HasVisibleCargo.Not().And(base.ViewModel.HasAnyCargo).ToReactiveProperty()
				.Subscribe(SetCargoEmptyText));
		}
		m_SorterDropdown.Or(null)?.Bind(base.ViewModel.SorterDropdownVM);
		if (m_SorterDropdown != null)
		{
			AddDisposable(m_SorterDropdown.Index.Subscribe(delegate
			{
				OnSorterDropdownValueChanged();
			}));
		}
		AddDisposable(base.ViewModel.HasVisibleCargo.Subscribe(SetEmptyCargoText));
		AddDisposable(base.ViewModel.SlotToScroll.Subscribe(ScrollToCargoItemDelayed));
	}

	public void OnSorterDropdownValueChanged()
	{
		int value = m_SorterDropdown.Index.Value;
		ItemsSorterType currentSorter = ((ItemsSorterType[])Enum.GetValues(typeof(ItemsSorterType)))[value];
		base.ViewModel.SetCurrentSorter(currentSorter);
	}

	public void SetDropdownState(bool value)
	{
		m_SorterDropdown.SetState(value);
	}

	private void SetCargoEmptyText(bool hasEnyCargo)
	{
		if (RootUIContext.Instance.IsVendorShow)
		{
			m_EmptyCargoText.text = (hasEnyCargo ? UIStrings.Instance.Vendor.NoValidCargos.Text : UIStrings.Instance.CargoTexts.EmptyCargo.Text);
		}
		else
		{
			m_EmptyCargoText.text = UIStrings.Instance.CargoTexts.EmptyCargo.Text;
		}
	}

	protected void SetEmptyCargoText(bool value)
	{
		if (m_EmptyCargo != null)
		{
			m_EmptyCargo.SetActive(!value);
		}
		m_VirtualList.gameObject.SetActive(value);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		if (CargoZoneView != null)
		{
			CargoZoneView.Hide();
		}
	}

	public void ChangeCargoView()
	{
		if (base.ViewModel.HasVisibleCargo.Value)
		{
			if (!base.ViewModel.IsCargoDetailedZone.Value)
			{
				m_CargoZoneView.Bind(base.ViewModel.CargoZoneVM);
				m_CargoZoneView.gameObject.SetActive(m_CargoZoneView.HasCargo.Value);
				m_ListContentFadeAnimator.PlayAnimation(!m_CargoZoneView.gameObject.activeSelf);
				base.ViewModel.IsCargoDetailedZone.Value = true;
				OnCargoViewChange.Execute();
				OnDetailedCargoShown?.Execute();
			}
			else
			{
				base.ViewModel.IsCargoDetailedZone.Value = false;
				m_CargoZoneView.gameObject.SetActive(value: false);
				m_ListContentFadeAnimator.AppearAnimation();
				OnCargoViewChange.Execute();
			}
		}
	}

	private void Show()
	{
		if (base.ViewModel.CargoViewType == InventoryCargoViewType.Loot)
		{
			m_FadeAnimator.AppearAnimation();
		}
		if (base.ViewModel.FromPointOfInterest)
		{
			m_ListContentFadeAnimator.AppearAnimation();
		}
	}

	private void Hide()
	{
		if (base.ViewModel.CargoViewType == InventoryCargoViewType.Loot)
		{
			m_FadeAnimator.DisappearAnimation();
		}
		if (base.ViewModel.FromPointOfInterest)
		{
			m_ListContentFadeAnimator.DisappearAnimation();
		}
	}

	public void SetList(bool val)
	{
		if (val)
		{
			DelayedInvoker.InvokeInFrames(ListSetup, 1);
		}
	}

	private void ListSetup()
	{
		m_CargoZoneView.Bind(base.ViewModel.CargoZoneVM);
		m_CargoZoneView.gameObject.SetActive(value: true);
		m_ListContentFadeAnimator.PlayAnimation(value: false);
		base.ViewModel.IsCargoDetailedZone.Value = m_CargoZoneView.HasCargo.Value;
	}

	private void ScrollToCargoItemDelayed(CargoSlotVM cargoSlotVM)
	{
		if (cargoSlotVM != null)
		{
			m_ScrollToDisposable?.Dispose();
			m_ScrollToDisposable = DelayedInvoker.InvokeInTime(delegate
			{
				ScrollToCargo(cargoSlotVM);
			}, m_ScrollToCargoDelay);
		}
	}

	private void ScrollToCargo(CargoSlotVM cargoSlotVM)
	{
		m_VirtualList.ScrollController.ForceScrollToElement(cargoSlotVM);
		DelayedInvoker.InvokeInFrames(delegate
		{
			HighlightCargo(cargoSlotVM);
		}, 1);
	}

	private void HighlightCargo(CargoSlotVM cargoSlotVM)
	{
		(m_VirtualList.Elements.FirstOrDefault((VirtualListElement e) => e?.Data == cargoSlotVM)?.View as CargoSlotConsoleView).Or(null)?.Highlight();
	}
}
