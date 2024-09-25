using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.DragNDrop;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarSlotAbilityPCView : SurfaceActionBarSlotAbilityView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	[Header("Drag'n'Drop")]
	[SerializeField]
	private DragNDropHandler m_DragNDropHandler;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotPCView.Bind(base.ViewModel);
		SetupDragNDrop();
	}

	private void SetupDragNDrop()
	{
		if ((bool)m_DragNDropHandler)
		{
			AddDisposable(base.ViewModel.IsEmpty.Subscribe(delegate(bool empty)
			{
				BaseUnitEntity baseUnitEntity = base.ViewModel.MechanicActionBarSlot?.Unit;
				m_DragNDropHandler.CanDrag = !empty && baseUnitEntity != null && (baseUnitEntity.IsMyNetRole() || baseUnitEntity.InPartyAndControllable());
			}));
			AddDisposable(m_DragNDropHandler.OnDragEnd.Subscribe(OnDragEnd));
			AddDisposable(base.ViewModel.UpdateDragAndDropState.Subscribe(delegate
			{
				UpdateDragAndDropStateNet();
			}));
		}
	}

	private void OnDragEnd(GameObject dropTarget)
	{
		SurfaceActionBarSlotAbilityPCView targetSlot = dropTarget.Or(null)?.GetComponent<SurfaceActionBarSlotAbilityPCView>();
		if ((bool)targetSlot)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(base.ViewModel.MechanicActionBarSlot, base.Index, targetSlot.Index);
			});
		}
		else if (base.ViewModel.IsInCharScreen)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.DeleteSlot(base.Index);
			});
		}
	}

	public void SetKeyBinding(int index)
	{
		m_SlotPCView.SetKeyBinding(index);
	}

	public override void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_SlotPCView.SetTooltipCustomPosition(rectTransform, pivots);
	}

	public void UpdateDragAndDropStateNet()
	{
		m_DragNDropHandler.CanDrag = !base.ViewModel.IsEmpty.Value && base.ViewModel.MechanicActionBarSlot?.Unit != null && base.ViewModel.MechanicActionBarSlot.Unit.IsMyNetRole();
	}
}
