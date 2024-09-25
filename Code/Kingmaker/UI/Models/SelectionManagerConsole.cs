using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Selection.UnitMark;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.Models;

public sealed class SelectionManagerConsole : SelectionManagerBase, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAreaLoadingStagesHandler
{
	private readonly List<BaseUnitEntity> m_LinkedUnits = new List<BaseUnitEntity>();

	public bool StopMoveFlag;

	protected override bool CanMultiSelect => false;

	public new static SelectionManagerConsole Instance => SelectionManagerBase.Instance as SelectionManagerConsole;

	public override void SelectUnit(UnitEntityView unit, bool single = true, bool sendSelectionEvent = true, bool ask = true)
	{
		if (unit == null)
		{
			return;
		}
		BaseUnitEntity entityData = unit.EntityData;
		if (!entityData.IsDirectlyControllable)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)entityData, (Action<ITrySelectNotControllableHandler>)delegate(ITrySelectNotControllableHandler h)
			{
				h.HandleSelectNotControllable(single, ask);
			}, isCheckRuntime: true);
		}
		else
		{
			if (!entityData.IsDirectlyControllable())
			{
				return;
			}
			base.SelectedUnit.Value = entityData;
			UpdateGamepadSelected();
			if (sendSelectionEvent)
			{
				EventBus.RaiseEvent((IBaseUnitEntity)entityData, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
				{
					h.OnUnitSelectionAdd(single, ask);
				}, isCheckRuntime: true);
			}
			if (Game.Instance.Player.IsInCombat)
			{
				base.SelectedUnits.ForEach(delegate(BaseUnitEntity u)
				{
					u.IsSelected = false;
				});
				base.SelectedUnits.Clear();
				base.SelectedUnits.Add(entityData);
				entityData.IsSelected = true;
				m_UnitMarks.ForEach(delegate(BaseUnitMark m)
				{
					m.Selected(IsSelected(m.Unit));
				});
			}
			else
			{
				SetSelectedUnitsToController();
				m_UnitMarks.ForEach(delegate(BaseUnitMark m)
				{
					m.Selected(IsSelected(m.Unit));
				});
			}
		}
	}

	protected override void SelectAllImpl(IEnumerable<BaseUnitEntity> units)
	{
		Game.Instance.State.AllBaseUnits.ForEach(delegate(BaseUnitEntity u)
		{
			u.IsLink = false;
		});
		units.ForEach(delegate(BaseUnitEntity u)
		{
			u.IsLink = true;
		});
		base.SelectedUnit.Value = (units.Contains(base.SelectedUnit.Value) ? base.SelectedUnit.Value : units.FirstOrDefault());
		UpdateLinkedUnits(force: true);
		UpdateGamepadSelected();
	}

	private void UpdateGamepadSelected()
	{
		m_UnitMarks.ForEach(delegate(BaseUnitMark m)
		{
			m.SetGamepadSelected(m.Unit == base.SelectedUnit.Value);
		});
	}

	public override void UnselectUnit(BaseUnitEntity data)
	{
		bool flag = data != null && data == base.SelectedUnit.Value;
		if (flag)
		{
			base.SelectedUnit.Value = null;
		}
		UpdateLinkedUnits(flag);
		if (data != null)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)data, (Action<ISelectionHandler>)delegate(ISelectionHandler h)
			{
				h.OnUnitSelectionRemove();
			}, isCheckRuntime: true);
		}
	}

	private void UpdateLinkedUnits(bool force = false)
	{
		if (Game.Instance.State.AllBaseUnits.Count((BaseUnitEntity u) => u.IsDirectlyControllable() && u.IsLink) != m_LinkedUnits.Count || force)
		{
			m_LinkedUnits.Clear();
			m_LinkedUnits.AddRange(Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity u) => u.IsDirectlyControllable() && u.IsLink));
			SetSelectedUnitsToController();
			UpdateUnitMarks();
			m_UnitMarks.ForEach(delegate(BaseUnitMark m)
			{
				m.Selected(IsSelected(m.Unit));
			});
		}
	}

	public bool IsLinked(BaseUnitEntity data)
	{
		return m_LinkedUnits.Contains(data);
	}

	public override void UpdateSelectedUnits()
	{
		UpdateLinkedUnits();
	}

	public override void ChangeNetRole(string entityId)
	{
		if (base.SelectedUnits.Count == 0)
		{
			SelectAll();
		}
		else
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.State.AllBaseUnits.FirstOrDefault((BaseUnitEntity u) => u.UniqueId == entityId && u.IsDirectlyControllable());
			if (baseUnitEntity != null)
			{
				baseUnitEntity.IsLink = true;
			}
			UpdateLinkedUnits(force: true);
			if (base.SelectedUnit.Value == null || base.SelectedUnit.Value.UniqueId == entityId)
			{
				base.SelectedUnit.Value = base.SelectedUnits.FirstOrDefault();
			}
		}
		BaseUnitEntity baseUnitEntity2 = m_LinkedUnits.Find((BaseUnitEntity u) => u.UniqueId == entityId);
		if (baseUnitEntity2 != null)
		{
			Game.Instance.SynchronizedDataController.PushLeftStickMovement(baseUnitEntity2, Vector2.zero, 0f);
		}
	}

	public void HandleAddCompanion()
	{
		OnAddCompanion(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleCompanionActivated()
	{
		OnAddCompanion(EventInvokerExtensions.BaseUnitEntity);
	}

	private void OnAddCompanion(BaseUnitEntity unit)
	{
		if (unit != null && unit.IsLink)
		{
			LinkUnit(unit);
		}
		DelayedInvoker.InvokeInFrames(delegate
		{
			UpdateLinkedUnits();
		}, 1);
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		UpdateLinkedUnits();
		if (base.SelectedUnit.Value == baseUnitEntity)
		{
			BaseUnitEntity baseUnitEntity2 = m_LinkedUnits.FirstOrDefault() ?? base.SelectedUnits.FirstOrDefault();
			if (baseUnitEntity2 != null)
			{
				SelectUnit(baseUnitEntity2.View, single: true, sendSelectionEvent: true, ask: false);
			}
		}
	}

	public void HandleCapitalModeChanged()
	{
		UpdateLinkedUnits();
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		SelectAll();
		UpdateLinkedUnits();
	}

	protected override void InternalDisable()
	{
		base.InternalDisable();
		if (Application.isPlaying)
		{
			m_LinkedUnits.Clear();
		}
	}

	protected override void InternalEnable()
	{
		base.InternalEnable();
		UpdateLinkedUnits();
	}

	public void SetMassLink(List<BaseUnitEntity> units)
	{
		Action<BaseUnitEntity> action = (m_LinkedUnits.Empty() ? new Action<BaseUnitEntity>(LinkUnit) : new Action<BaseUnitEntity>(UnlinkUnit));
		units.ForEach(action);
	}

	public void LinkUnit(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			unit.IsLink = true;
		}
		UpdateLinkedUnits();
	}

	public void UnlinkUnit(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			unit.IsLink = false;
		}
		UpdateLinkedUnits();
	}

	public override void Stop()
	{
		StopMoveFlag = true;
		base.Stop();
	}

	private void SetSelectedUnitsToController()
	{
		base.SelectedUnits.ForEach(delegate(BaseUnitEntity u)
		{
			u.IsSelected = false;
		});
		base.SelectedUnits.Clear();
		BaseUnitEntity value = base.SelectedUnit.Value;
		if (m_LinkedUnits.Contains(value) || value == null)
		{
			foreach (BaseUnitEntity linkedUnit in m_LinkedUnits)
			{
				if (linkedUnit.IsDirectlyControllable())
				{
					base.SelectedUnits.Add(linkedUnit);
				}
			}
		}
		else if (value != null && value.IsDirectlyControllable())
		{
			base.SelectedUnits.Add(value);
		}
		base.SelectedUnits.ForEach(delegate(BaseUnitEntity u)
		{
			u.IsSelected = true;
		});
	}

	public override void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			Game.Instance.State.AllBaseUnits.ForEach(delegate(BaseUnitEntity u)
			{
				u.IsLink = false;
			});
			UpdateLinkedUnits();
		}
	}

	public override void HandleTurnBasedModeResumed()
	{
		Game.Instance.State.AllBaseUnits.ForEach(delegate(BaseUnitEntity u)
		{
			u.IsLink = false;
		});
		UpdateLinkedUnits();
	}

	public override void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			UnselectUnit(base.SelectedUnit.Value);
		}
	}
}
