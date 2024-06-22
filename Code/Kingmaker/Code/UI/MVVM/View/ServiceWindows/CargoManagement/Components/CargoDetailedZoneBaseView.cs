using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoDetailedZoneBaseView : ViewBase<CargoDetailedZoneVM>
{
	[Header("Slots")]
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[SerializeField]
	private CargoDetailedBaseView m_CargoDetailedPrefab;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private Transform m_Panel;

	[Header("Search Part")]
	[SerializeField]
	public ItemsFilterSearchBaseView SearchView;

	[SerializeField]
	private float m_ScrollToCargoDelay = 0.2f;

	public List<CargoDetailedPCView> m_CargoList = new List<CargoDetailedPCView>();

	public BoolReactiveProperty HasCargo = new BoolReactiveProperty();

	public readonly ReactiveCommand OnEnableSlotsChange = new ReactiveCommand();

	public readonly ReactiveCommand NeedRefocus = new ReactiveCommand();

	public readonly ReactiveCommand<CargoSlotVM> ScrolledEntity = new ReactiveCommand<CargoSlotVM>();

	private IDisposable m_ScrollToDisposable;

	public void Initialize()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<CargoSlotVM>(m_CargoDetailedPrefab));
		m_FadeAnimator.Initialize();
		SearchView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		HasCargo.Value = base.ViewModel.CargoSlots.Any((CargoSlotVM slot) => !slot.IsEmpty);
		m_FadeAnimator.PlayAnimation(HasCargo.Value);
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.CargoSlots));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.CargoSlots.ObserveAnyCollectionChange().Skip(1), delegate
		{
			OnEnableSlotsChange?.Execute();
		}));
		AddDisposable(base.ViewModel.CargoSlots.ObserveCountChanged().Skip(1).Subscribe(delegate
		{
			NeedRefocus?.Execute();
		}));
		SearchView.Or(null)?.Bind(base.ViewModel.ItemsFilterSearchVM);
		if (SearchView != null)
		{
			AddDisposable(HasCargo.Subscribe(SearchView.gameObject.SetActive));
		}
		AddDisposable(base.ViewModel.SlotToScroll.Subscribe(ScrollToCargoItemDelayed));
	}

	protected override void DestroyViewImplementation()
	{
		m_CargoList.Clear();
		m_FadeAnimator.DisappearAnimation();
	}

	public void Hide()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	private void ScrollToCargoItemDelayed(CargoSlotVM cargoSlotVM)
	{
		if (cargoSlotVM != null)
		{
			CargoSlotVM slotVM = base.ViewModel.CargoSlots.FindOrDefault((CargoSlotVM x) => x.CargoEntity == cargoSlotVM.CargoEntity);
			m_ScrollToDisposable?.Dispose();
			m_ScrollToDisposable = DelayedInvoker.InvokeInTime(delegate
			{
				ScrollToCargo(slotVM ?? cargoSlotVM);
			}, m_ScrollToCargoDelay);
			DelayedInvoker.InvokeInFrames(delegate
			{
				slotVM?.Blink();
			}, 5);
		}
	}

	private void ScrollToCargo(CargoSlotVM cargoSlotVM)
	{
		m_VirtualList.ScrollController.ForceScrollToElement(cargoSlotVM);
		ScrolledEntity?.Execute(cargoSlotVM);
	}

	public void ScrollToTop()
	{
		m_VirtualList.ScrollController.ForceScrollToTop();
	}
}
