using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Controllers;

public class AreaEffectsController : IControllerTick, IController, ITeleportHandler, ISubscriber, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnBasedModeHandler
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			areaEffect.Tick();
		}
	}

	private static void TickAreaEffects(bool isTurnBased, Initiative.Event @event)
	{
		if (!isTurnBased && @event != 0)
		{
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Initiative.ShouldActNow(isTurnBased, @event, out var actRound))
			{
				areaEffect.Initiative.LastTurn = actRound;
				areaEffect.NextRound();
			}
		}
	}

	void ITeleportHandler.HandlePartyTeleport(AreaEnterPoint enterPoint)
	{
		Tick();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.TurnStart);
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.RoundStart);
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		TickAreaEffects(isTurnBased, Initiative.Event.RoundEnd);
	}

	public static AreaEffectEntity Spawn(MechanicsContext parentContext, BlueprintAbilityAreaEffect blueprint, TargetWrapper target, TimeSpan? duration)
	{
		return Spawn(parentContext, blueprint, target, duration, onUnit: false);
	}

	public static AreaEffectEntity SpawnAttachedToTarget(MechanicsContext parentContext, BlueprintAbilityAreaEffect blueprint, BaseUnitEntity target, TimeSpan? duration)
	{
		return Spawn(parentContext, blueprint, target, duration, onUnit: true);
	}

	private static AreaEffectEntity Spawn(MechanicsContext parentContext, BlueprintAbilityAreaEffect blueprint, TargetWrapper target, TimeSpan? duration, bool onUnit)
	{
		SceneEntitiesState state = ((!onUnit) ? Game.Instance.LoadedAreaState.MainState : (target.Entity?.HoldingState ?? ContextData<EntitySpawnController.EntitySpawnData>.Current?.TargetState));
		AreaEffectView areaEffectView = new GameObject($"AreaEffect [{blueprint}]").AddComponent<AreaEffectView>();
		areaEffectView.UniqueId = Uuid.Instance.CreateString();
		areaEffectView.OnUnit = onUnit;
		areaEffectView.InitAtRuntime(parentContext.CloneFor(blueprint, null, null, target), blueprint, target, Game.Instance.TimeController.GameTime, duration);
		AreaEffectEntity obj = (AreaEffectEntity)Game.Instance.EntitySpawner.SpawnEntityWithView(areaEffectView, state);
		EventBus.RaiseEvent((IAreaEffectEntity)obj, (Action<IAreaEffectHandler>)delegate(IAreaEffectHandler h)
		{
			h.HandleAreaEffectSpawned();
		}, isCheckRuntime: true);
		return obj;
	}

	public static bool CheckConcussionEffect(CustomGridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasConcussionEffect)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckCantAttackEffect(CustomGridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasCantAttackEffect)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckInertWarpEffect(CustomGridNodeBase node)
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Contains(node) && areaEffect.Blueprint.HasInertWarpEffect)
			{
				return true;
			}
		}
		return false;
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint.OnlyInCombat)
			{
				areaEffect.ForceEnd();
			}
		}
	}
}
