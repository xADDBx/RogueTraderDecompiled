using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Controllers;

public class EntityDestructionController : IControllerTick, IController, IControllerStop
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("EntityDestructionController");

	[ItemNotNull]
	private readonly HashSet<Entity> m_ToDestroy = new HashSet<Entity>();

	private bool m_CleanupAwakeUnits;

	public void Destroy([CanBeNull] Entity entity)
	{
		if (entity == null || (entity.HoldingState == null && Game.Instance.EntitySpawner.TryRemoveFromSpawnQueue(entity)))
		{
			entity?.Dispose();
			return;
		}
		int currentDepth = ContextData<UnitHelper.UnitBeingDestroyed>.CurrentDepth;
		if (currentDepth >= 0)
		{
			if (currentDepth >= 5)
			{
				Logger.ErrorWithReport("Infinite destroy loop detected!");
			}
			else
			{
				DestroyImmediate(entity);
			}
			return;
		}
		entity.WillBeDestroyed = true;
		if (NeedFadeOut(entity))
		{
			entity.GetOrCreate<PartFadeOutAndDestroy>().Setup(1.8f);
		}
		else
		{
			m_ToDestroy.Add(entity);
		}
	}

	public void OnStop()
	{
		m_ToDestroy.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (Entity item in m_ToDestroy)
		{
			item.WillBeDestroyed = false;
			if (!DestroyImmediate(item))
			{
				return;
			}
		}
		if (m_CleanupAwakeUnits)
		{
			Game.Instance.GetController<SleepingUnitsController>()?.Tick();
			m_CleanupAwakeUnits = false;
		}
		m_ToDestroy.Clear();
	}

	private bool DestroyImmediate(Entity entity)
	{
		if (entity.HoldingState == null)
		{
			return true;
		}
		if (entity.IsDisposed)
		{
			Logger.ErrorWithReport("Disposed entity in the game state!");
			entity.HoldingState.RemoveEntityData(entity);
			return false;
		}
		try
		{
			PerformDestroy(entity);
			CheckCleanUp(entity as BaseUnitEntity);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
		return true;
	}

	private static bool NeedFadeOut(Entity entity)
	{
		if (entity.ViewHandlingOnDisposePolicy == Entity.ViewHandlingOnDisposePolicyType.FadeOutAndDestroy && entity.View != null)
		{
			return entity.GetOptional<PartFadeOutAndDestroy>() == null;
		}
		return false;
	}

	private static void PerformDestroy(Entity entity)
	{
		using (ContextData<UnitHelper.UnitBeingDestroyed>.Request())
		{
			if (entity is BaseUnitEntity baseUnitEntity)
			{
				bool flag = baseUnitEntity.GetOptional<UnitPartSummonedMonster>();
				if (baseUnitEntity.Faction.IsPlayer && !baseUnitEntity.IsPet && !flag && TryUnrecruit(baseUnitEntity))
				{
					Logger.Error($"Cancel unit's destruction: {baseUnitEntity}");
					return;
				}
			}
			entity.Remove<PartFadeOutAndDestroy>();
			entity.HoldingState?.RemoveEntityData(entity);
			entity.HandleDestroy();
			entity.Dispose();
		}
	}

	private void CheckCleanUp(BaseUnitEntity unit)
	{
		if (!m_CleanupAwakeUnits && unit != null && Game.Instance.State.AllBaseAwakeUnits.HasItem(unit))
		{
			m_CleanupAwakeUnits = true;
		}
	}

	private static bool TryUnrecruit(BaseUnitEntity unit)
	{
		if ((bool)unit.GetOptional<UnitPartCompanion>() && !unit.Facts.HasComponent<TransientPartyMemberFlag>())
		{
			Logger.Error($"Trying to destroy {unit} who is a companion. Do not do that!");
			unit.GetOptional<UnitPartCompanion>()?.SetState(CompanionState.ExCompanion);
			UIAccess.SelectionManager.UpdateSelectedUnits();
			unit.IsInGame = false;
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
			{
				h.HandleUnrecruit();
			}, isCheckRuntime: true);
			return true;
		}
		if (unit.IsPet)
		{
			Logger.Error($"trying to destroy {unit} who is a companion pet. Do not do that!");
			unit.IsInGame = false;
			return true;
		}
		return false;
	}
}
