using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.UI.Selection;

public class AggroRangeMarkController : MonoBehaviour, IViewAttachedHandler, ISubscriber<IEntity>, ISubscriber, IViewDetachedHandler, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IUnitFactionHandler, IUnitChangeAttackFactionsHandler, IUnitBecameVisibleHandler, IUnitBecameInvisibleHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IGameModeHandler
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("AggroRangeMarkController");

	public AggroRangeMark AggroRangeMarker;

	private readonly IDictionary<MechanicEntity, AggroRangeMark> m_Marks = new Dictionary<MechanicEntity, AggroRangeMark>();

	private static bool IsCutscene => Game.Instance.CurrentMode == GameModeType.Cutscene;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleUnitDeath(BaseUnitEntity unit)
	{
		if (m_Marks.TryGetValue(unit, out var value))
		{
			UpdateVisibility(unit, value.gameObject);
		}
	}

	private void AddEntity(UnitEntity unit, GameObject view)
	{
		AggroRangeMark aggroRangeMark = view.transform.GetComponentInChildren<AggroRangeMark>();
		if (!aggroRangeMark)
		{
			aggroRangeMark = Object.Instantiate(AggroRangeMarker, view.transform);
		}
		Transform obj = aggroRangeMark.transform;
		Transform parent = obj.parent;
		obj.parent = null;
		obj.localScale = Vector3.one * unit.Vision.RangeMeters;
		obj.parent = parent;
		obj.localPosition = Vector3.zero;
		obj.localRotation = Quaternion.identity;
		aggroRangeMark.gameObject.hideFlags = HideFlags.DontSave;
		m_Marks.Add(unit, aggroRangeMark);
		UpdateVisibility(unit, aggroRangeMark.gameObject);
	}

	private static void UpdateVisibility(BaseUnitEntity data, GameObject gameObject)
	{
		bool active = !IsCutscene && !data.CombatState.IsInCombat && data.CombatGroup.IsEnemy(Game.Instance.Player.Group) && data.LifeState.IsConscious && data.IsVisibleForPlayer;
		gameObject.SetActive(active);
	}

	private void RemoveEntity(UnitEntity unit)
	{
		if (!m_Marks.TryGetValue(unit, out var value))
		{
			Logger.Error("Cant find mark for unit {0}", unit);
		}
		else
		{
			Object.Destroy(value.gameObject);
			m_Marks.Remove(unit);
		}
	}

	public void HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (m_Marks.TryGetValue(baseUnitEntity, out var value))
		{
			UpdateVisibility(baseUnitEntity, value.gameObject);
		}
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (m_Marks.TryGetValue(baseUnitEntity, out var value))
		{
			UpdateVisibility(baseUnitEntity, value.gameObject);
		}
	}

	public void HandleFactionChanged()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (m_Marks.TryGetValue(baseUnitEntity, out var value))
		{
			UpdateVisibility(baseUnitEntity, value.gameObject);
		}
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit1)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (m_Marks.TryGetValue(baseUnitEntity, out var value))
		{
			UpdateVisibility(baseUnitEntity, value.gameObject);
		}
	}

	public void OnViewAttached(IEntityViewBase view)
	{
		if (EventInvokerExtensions.Entity is UnitEntity unit)
		{
			AddEntity(unit, view.GO);
		}
	}

	public void OnViewDetached(IEntityViewBase view)
	{
		if (EventInvokerExtensions.Entity is UnitEntity unit)
		{
			RemoveEntity(unit);
		}
	}

	public void OnEntityBecameVisible()
	{
		UpdateVisibilityForContextUnit();
	}

	public void OnEntityBecameInvisible()
	{
		UpdateVisibilityForContextUnit();
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && m_Marks.TryGetValue(baseUnitEntity, out var value))
		{
			UpdateVisibility(baseUnitEntity, value.gameObject);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		UpdateVisibilityForAll();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		UpdateVisibilityForAll();
	}

	private void UpdateVisibilityForContextUnit()
	{
		if (EventInvokerExtensions.Entity is UnitEntity unitEntity && m_Marks.TryGetValue(unitEntity, out var value))
		{
			UpdateVisibility(unitEntity, value.gameObject);
		}
	}

	private void UpdateVisibilityForAll()
	{
		m_Marks.ForEach(delegate(KeyValuePair<MechanicEntity, AggroRangeMark> m)
		{
			UpdateVisibility(m.Key as UnitEntity, m.Value.gameObject);
		});
	}
}
