using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UI.VirtualListSystem;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class SelectorWindowBaseView<TEntityView, TEntityVM> : ViewBase<SelectorWindowVM<TEntityVM>> where TEntityView : VirtualListElementViewBase<TEntityVM> where TEntityVM : SelectionGroupEntityVM
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TextMeshProUGUI m_Header;

	[SerializeField]
	protected VirtualListComponent m_VirtualList;

	[SerializeField]
	protected TEntityView m_SlotPrefab;

	[SerializeField]
	protected InfoSectionView m_InfoSectionView;

	[SerializeField]
	protected CanvasSortingComponent m_SortingComponent;

	protected readonly BoolReactiveProperty CanEquip = new BoolReactiveProperty(initialValue: true);

	protected readonly BoolReactiveProperty m_SelectedEquipped = new BoolReactiveProperty();

	protected bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_VirtualList.Initialize(new VirtualListElementTemplate<TEntityVM>(m_SlotPrefab));
			m_InfoSectionView.Initialize();
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		m_FadeAnimator.AppearAnimation();
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.EntitiesCollection));
		m_InfoSectionView.Bind(base.ViewModel.InfoSectionVM);
		AddDisposable(m_SelectedEquipped.Subscribe(delegate(bool value)
		{
			CanEquip.Value = !value && TakeControllableCharacter();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	protected bool TakeControllableCharacter()
	{
		return ((BaseUnitEntity)(base.ViewModel.Slot?.ItemSlot?.Owner))?.CanBeControlled() ?? true;
	}

	protected void OnClose(InputActionEventData inputActionEventData)
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		else
		{
			base.ViewModel.Back();
		}
	}

	protected virtual void OnClose()
	{
		if (RootUIContext.Instance.TooltipIsShown)
		{
			TooltipHelper.HideTooltip();
		}
		else
		{
			base.ViewModel.Back();
		}
	}
}
