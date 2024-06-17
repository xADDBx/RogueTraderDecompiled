using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark.Managers;

public class UnitMarkManager : MonoBehaviour, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, IUnitChangeAttackFactionsHandler, IViewDetachedHandler, ISubscriber<IEntity>, IViewAttachedHandler
{
	public BaseUnitMark UnitMarkPrefab;

	private readonly Dictionary<AbstractUnitEntity, BaseUnitMark> m_Marks = new Dictionary<AbstractUnitEntity, BaseUnitMark>();

	public void OnEnable()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			AbstractUnitEntityView view = allUnit.View;
			if ((bool)view && UnitNeedsMark(allUnit))
			{
				AddMark(allUnit, view);
			}
		}
		EventBus.Subscribe(this);
	}

	public void OnDisable()
	{
		EventBus.Unsubscribe(this);
		foreach (var (abstractUnitEntity2, baseUnitMark2) in m_Marks)
		{
			if ((bool)baseUnitMark2)
			{
				baseUnitMark2.Dispose();
				Utils.EditorSafeDestroy(baseUnitMark2.gameObject);
				abstractUnitEntity2.View.MarkRenderersAndCollidersAreUpdated();
			}
		}
		m_Marks.Clear();
	}

	protected virtual bool UnitNeedsMark(AbstractUnitEntity unit)
	{
		return !unit.LifeState.IsDead;
	}

	private void AddMark(AbstractUnitEntity unit, AbstractUnitEntityView view)
	{
		if (!m_Marks.ContainsKey(unit))
		{
			BaseUnitMark baseUnitMark = Object.Instantiate(UnitMarkPrefab, unit.View.ViewTransform);
			baseUnitMark.transform.localPosition = Vector3.zero;
			baseUnitMark.transform.localRotation = Quaternion.identity;
			baseUnitMark.Initialize(unit);
			m_Marks.Add(unit, baseUnitMark);
			view.MarkRenderersAndCollidersAreUpdated();
		}
	}

	private void RemoveMark(AbstractUnitEntity unit)
	{
		if (m_Marks.TryGetValue(unit, out var value))
		{
			value.Dispose();
			Utils.EditorSafeDestroy(value.gameObject);
			unit.View.MarkRenderersAndCollidersAreUpdated();
			m_Marks.Remove(unit);
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		HandleUpdateUnitState();
	}

	public void HandleFactionChanged()
	{
		HandleUpdateUnitState();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		HandleUpdateUnitState();
	}

	private void HandleUpdateUnitState()
	{
		AbstractUnitEntity unit = EventInvokerExtensions.AbstractUnitEntity;
		DelayedInvoker.InvokeInFrames(delegate
		{
			UpdateProperties(unit);
		}, 1);
	}

	private void UpdateProperties(AbstractUnitEntity unit)
	{
		if (UnitNeedsMark(unit) && (bool)unit.View)
		{
			AddMark(unit, unit.View);
		}
		else
		{
			RemoveMark(unit);
		}
	}

	public void OnViewDetached(IEntityViewBase view)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity unit)
		{
			RemoveMark(unit);
		}
	}

	public void OnViewAttached(IEntityViewBase viewBase)
	{
		if (EventInvokerExtensions.Entity is AbstractUnitEntity unit && viewBase is AbstractUnitEntityView view && UnitNeedsMark(unit))
		{
			AddMark(unit, view);
		}
	}
}
