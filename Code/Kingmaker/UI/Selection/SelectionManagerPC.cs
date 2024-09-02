using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Selection.UnitMark;
using Kingmaker.View;

namespace Kingmaker.UI.Selection;

public sealed class SelectionManagerPC : SelectionManagerBase, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAreaLoadingStagesHandler
{
	public void HandleAddCompanion()
	{
		StartCoroutine(AddRemoveCompanion(EventInvokerExtensions.BaseUnitEntity));
	}

	public void HandleCompanionActivated()
	{
		StartCoroutine(AddRemoveCompanion());
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		StartCoroutine(AddRemoveCompanion());
	}

	public void HandleCapitalModeChanged()
	{
		StartCoroutine(AddRemoveCompanion());
	}

	public override void SelectUnit(UnitEntityView unit, bool single = true, bool sendSelectionEvent = true, bool ask = true)
	{
		if (unit == null || !unit.EntityData.IsDirectlyControllable())
		{
			return;
		}
		Game.Instance?.ClickEventsController?.ClearPointerMode();
		DragNDropManager.Instance?.CancelDrag();
		BaseUnitEntity selectedUnit = unit.EntityData;
		if (single)
		{
			Clear();
		}
		if (!selectedUnit.IsDirectlyControllable)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)selectedUnit, (Action<ITrySelectNotControllableHandler>)delegate(ITrySelectNotControllableHandler h)
			{
				h.HandleSelectNotControllable(single, ask);
			}, isCheckRuntime: true);
			return;
		}
		m_UnitMarks.Find((BaseUnitMark decal) => decal.Unit == selectedUnit)?.Selected(isSelected: true);
		base.SelectedUnits.Add(selectedUnit);
		base.SelectedUnit.Value = base.SelectedUnits.FirstOrDefault();
		selectedUnit.IsSelected = true;
		if (sendSelectionEvent)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)selectedUnit, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
			{
				h.OnUnitSelectionAdd(single, ask);
			}, isCheckRuntime: true);
		}
	}

	public override void UnselectUnit(BaseUnitEntity data)
	{
		if (base.SelectedUnits.Remove(data))
		{
			data.IsSelected = false;
			base.SelectedUnit.Value = base.SelectedUnits.FirstOrDefault();
			m_UnitMarks.Find((BaseUnitMark m) => m.Unit == data)?.Selected(isSelected: false);
			EventBus.RaiseEvent((IBaseUnitEntity)data, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
			{
				h.OnUnitSelectionRemove();
			}, isCheckRuntime: true);
		}
	}

	private void Clear()
	{
		foreach (BaseUnitEntity selected in base.SelectedUnits)
		{
			selected.IsSelected = false;
			m_UnitMarks.Find((BaseUnitMark mark) => mark.Unit == selected)?.Selected(isSelected: false);
			EventBus.RaiseEvent((IBaseUnitEntity)selected, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
			{
				h.OnUnitSelectionRemove();
			}, isCheckRuntime: true);
		}
		base.SelectedUnits.Clear();
	}

	public override void UpdateSelectedUnits()
	{
		List<UnitEntityView> views = new List<UnitEntityView>(from u in SelectionManagerBase.GetSelectableUnits(base.SelectedUnits)
			select u.View);
		MultiSelect(views);
	}

	public void MultiSelect(IEnumerable<UnitEntityView> views, bool canAddToSelection = true)
	{
		if (KeyboardAccess.IsShiftHold() && canAddToSelection)
		{
			foreach (UnitEntityView view in views)
			{
				if (!IsSelected(view.EntityData))
				{
					SelectUnit(view, single: false);
				}
			}
			return;
		}
		Clear();
		foreach (UnitEntityView view2 in views)
		{
			SelectUnit(view2, single: false, sendSelectionEvent: false);
		}
		foreach (BaseUnitEntity selectedUnit in base.SelectedUnits)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)selectedUnit, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
			{
				h.OnUnitSelectionAdd(single: false, ask: true);
			}, isCheckRuntime: true);
		}
	}

	private IEnumerator AddRemoveCompanion(BaseUnitEntity unit = null)
	{
		yield return null;
		UpdateUnitMarks();
		if (unit != null)
		{
			SelectUnit(unit.View, single: false);
		}
	}

	protected override void SelectAllImpl(IEnumerable<BaseUnitEntity> units)
	{
		IEnumerable<UnitEntityView> views = units.Select((BaseUnitEntity c) => c.View);
		MultiSelect(views, canAddToSelection: false);
	}

	public override void ChangeNetRole(string entityId)
	{
		if (!base.SelectedUnits.Any((BaseUnitEntity u) => u.IsDirectlyControllable()))
		{
			SelectAll();
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		SelectAll();
	}
}
