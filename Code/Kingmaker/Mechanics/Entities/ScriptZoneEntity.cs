using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public class ScriptZoneEntity : MapObjectEntity, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IAreaHandler, IHashable
{
	public class UnitInfo
	{
		public UnitReference Reference;

		public bool InsideThisTick;

		public bool IsValid
		{
			get
			{
				if (Reference.Entity != null)
				{
					return Reference.Entity.ToBaseUnitEntity().IsInState;
				}
				return false;
			}
		}
	}

	[JsonProperty]
	public bool IsActive = true;

	[JsonProperty]
	public bool WasEntered;

	public readonly List<UnitInfo> InsideUnits = new List<UnitInfo>();

	public new ScriptZone View => (ScriptZone)base.View;

	[UsedImplicitly]
	protected ScriptZoneEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public ScriptZoneEntity(MapObjectView view)
		: base(view)
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (!IsActive || !base.IsInGame)
		{
			return;
		}
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (IsInterestedInUnit(allBaseUnit) && ContainsPosition(allBaseUnit.Position))
			{
				TryAddUnit(allBaseUnit);
			}
			if (!IsActive)
			{
				break;
			}
		}
	}

	public void Tick()
	{
		if (!IsActive)
		{
			return;
		}
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			insideUnit.InsideThisTick = false;
		}
		foreach (IScriptZoneShape shape in View.Shapes)
		{
			List<BaseUnitEntity> list = (View.PlayersOnly ? Game.Instance.Player.PartyAndPets : (EntityBoundsHelper.FindUnitsInShape(shape) ?? Game.Instance.State.AllBaseAwakeUnits));
			using (ProfileScope.New("Tick one shape"))
			{
				foreach (BaseUnitEntity item in list)
				{
					TickUnit(item, shape);
					if (!IsActive)
					{
						break;
					}
				}
			}
		}
		foreach (UnitInfo item2 in InsideUnits.ToTempList())
		{
			if (!item2.InsideThisTick)
			{
				RemoveUnit(item2.Reference.ToBaseUnitEntity());
			}
		}
		InsideUnits.RemoveAll((UnitInfo i) => !i.IsValid);
	}

	private void TickUnit(BaseUnitEntity unit, IScriptZoneShape shape)
	{
		if (!IsActive || !IsInterestedInUnit(unit) || unit.IsExtra)
		{
			return;
		}
		UnitInfo unitInfo = null;
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			if (insideUnit.Reference == unit)
			{
				unitInfo = insideUnit;
				break;
			}
		}
		if ((unitInfo == null || !unitInfo.InsideThisTick) && shape.Contains(unit.Position) && !AbstractUnitCommand.CommandTargetUntargetable(null, unit))
		{
			if (unitInfo != null)
			{
				unitInfo.InsideThisTick = true;
			}
			else
			{
				TryAddUnit(unit);
			}
		}
	}

	public bool ContainsPosition(Vector3 point)
	{
		for (int i = 0; i < View.Shapes.Count; i++)
		{
			if (View.Shapes[i].Contains(point))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsUnit(AbstractUnitEntity unit)
	{
		if (!IsActive)
		{
			return false;
		}
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			if (insideUnit.Reference.Entity == unit)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInterestedInUnit(BaseUnitEntity unit)
	{
		if (unit.LifeState.IsDead)
		{
			return false;
		}
		if (View.PlayersOnly && !unit.Faction.IsPlayer)
		{
			return false;
		}
		return true;
	}

	private void TryAddUnit(BaseUnitEntity unit)
	{
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(unit))
		{
			if (View.Blueprint != null && !View.Blueprint.TriggerConditions.Check())
			{
				return;
			}
			InsideUnits.Add(new UnitInfo
			{
				Reference = unit.FromBaseUnitEntity(),
				InsideThisTick = true
			});
			if (View.DisableSameMultipleTriggers)
			{
				if (WasEntered)
				{
					return;
				}
				WasEntered = true;
			}
			OnUnitEnter(unit);
			View.OnUnitEntered.Invoke(unit, View);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IScriptZoneHandler>)delegate(IScriptZoneHandler h)
			{
				h.OnUnitEnteredScriptZone(View);
			}, isCheckRuntime: true);
		}
		if (View.OnceOnly)
		{
			IsActive = false;
		}
	}

	private void RemoveUnit(BaseUnitEntity unit)
	{
		InsideUnits.Remove((UnitInfo i) => i.Reference == unit);
		OnUnitRemoved(unit);
	}

	private void OnUnitRemoved(BaseUnitEntity unit)
	{
		if (View.DisableSameMultipleTriggers && WasEntered)
		{
			if (View.Count > 0)
			{
				return;
			}
			WasEntered = false;
		}
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(unit))
		{
			OnUnitExit(unit);
			View.OnUnitExited.Invoke(unit, View);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IScriptZoneHandler>)delegate(IScriptZoneHandler h)
			{
				h.OnUnitExitedScriptZone(View);
			}, isCheckRuntime: true);
		}
	}

	private void OnUnitExit(BaseUnitEntity triggeringUnit)
	{
		if (base.Blueprint != null)
		{
			if (View.Blueprint == null)
			{
				PFLog.Default.Warning("ScriptZone " + View.name + " blueprint not set");
			}
			else
			{
				View.Blueprint.ExitActions.Run();
			}
		}
	}

	private void OnUnitEnter(BaseUnitEntity triggeringUnit)
	{
		if (base.Blueprint != null)
		{
			if (View.Blueprint == null)
			{
				PFLog.Default.Warning("ScriptZone " + View.name + " blueprint not set");
			}
			else
			{
				View.Blueprint.EnterActions.Run();
			}
		}
	}

	public void HandleUnitSpawned()
	{
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		HandleUnitDestroyed(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitHandler.HandleUnitDeath()
	{
		HandleUnitDestroyed(EventInvokerExtensions.BaseUnitEntity);
	}

	private void HandleUnitDestroyed(BaseUnitEntity entityData)
	{
		if (entityData == null || !InsideUnits.HasItem((UnitInfo i) => i.Reference == entityData))
		{
			return;
		}
		if (IsActive)
		{
			RemoveUnit(entityData);
			return;
		}
		InsideUnits.Remove((UnitInfo i) => i.Reference == entityData);
	}

	public void RemoveAll()
	{
		foreach (UnitInfo item in InsideUnits.ToTempList())
		{
			RemoveUnit(item.Reference.ToBaseUnitEntity());
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsActive);
		result.Append(ref WasEntered);
		return result;
	}
}
