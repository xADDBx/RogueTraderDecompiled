using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Spawners;

namespace Kingmaker.Controllers.GlobalMap;

public class CombatRandomEncounterController : IControllerEnable, IController, IControllerDisable, IControllerTick, IAreaHandler, ISubscriber, IControllerReset
{
	private bool m_IsInRandomEncounter;

	private bool m_IsEnterPointsActive;

	void IControllerReset.OnReset()
	{
		m_IsInRandomEncounter = false;
		m_IsEnterPointsActive = false;
	}

	public void ActivateCombatRandomEncounter([NotNull] BlueprintAreaReference area, [CanBeNull] BlueprintAreaEnterPointReference enterPoint, [CanBeNull] BlueprintRandomGroupOfUnits.Reference group)
	{
		CombatRandomEncounterState combatRandomEncounterState = Game.Instance.Player.CombatRandomEncounterState;
		if (area.Get().GetComponent<AreaRandomEncounter>() == null)
		{
			PFLog.Default.Warning($"Cannot find component AreaRandomEncounter on {area}. Random generation will be skipped");
			combatRandomEncounterState.Area = area.Get();
			combatRandomEncounterState.EnterPoint = enterPoint?.Get();
			return;
		}
		int seed = (CheatsRE.RandomSeed.HasValue ? CheatsRE.RandomSeed.Value : Game.Instance.Player.GameId.GetHashCode());
		BlueprintAreaEnterPointReference blueprintAreaEnterPointReference = ((enterPoint?.Get() == null) ? CombatRandomEncountersGenerator.GenerateRandomEnterPoint(seed, area.Get()) : enterPoint);
		Dictionary<EntityReference, BlueprintUnit> dictionary = CombatRandomEncountersGenerator.GenerateArea(seed, area.Get(), blueprintAreaEnterPointReference, group?.Get());
		if (dictionary != null)
		{
			combatRandomEncounterState.Area = area.Get();
			combatRandomEncounterState.Spawners = dictionary;
			combatRandomEncounterState.EnterPoint = blueprintAreaEnterPointReference;
			combatRandomEncounterState.IsInCombatRandomEncounter = true;
			combatRandomEncounterState.CoverGroup = CombatRandomEncountersGenerator.GenerateCovers(area.Get());
			combatRandomEncounterState.TrapGroup = CombatRandomEncountersGenerator.GenerateTraps(area.Get());
			combatRandomEncounterState.AreaEffectGroup = CombatRandomEncountersGenerator.GenerateAreaEffects(area.Get());
			combatRandomEncounterState.OtherMapObjectGroup = CombatRandomEncountersGenerator.GenerateOtherMapObjects(area.Get());
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!m_IsEnterPointsActive && Game.Instance.CurrentlyLoadedArea == Game.Instance.Player.CombatRandomEncounterState.Area && !Game.Instance.State.AllBaseUnits.Any((BaseUnitEntity i) => i.Faction.IsPlayerEnemy && !i.IsDead && UnitCombatJoinController.CanJoinCombat(i)))
		{
			SetEnterPointsActive(active: true);
		}
	}

	public void OnAreaBeginUnloading()
	{
		CombatRandomEncounterState combatRandomEncounterState = Game.Instance.Player.CombatRandomEncounterState;
		if (!m_IsInRandomEncounter || !m_IsEnterPointsActive)
		{
			return;
		}
		m_IsInRandomEncounter = false;
		combatRandomEncounterState.UnlockFlag?.Lock();
		combatRandomEncounterState.ClearState();
		AreaPersistentState areaPersistentState = Game.Instance.State.SavedAreaStates.FirstOrDefault((AreaPersistentState areaState) => areaState.Blueprint == Game.Instance.CurrentlyLoadedArea);
		if (areaPersistentState != null)
		{
			Game.Instance.State.SavedAreaStates.Remove(areaPersistentState);
			EventBus.RaiseEvent(delegate(ICombatRandomEncounterHandler h)
			{
				h.HandleCombatRandomEncounterFinish();
			});
		}
	}

	public void OnAreaDidLoad()
	{
		if (!m_IsInRandomEncounter)
		{
			return;
		}
		CombatRandomEncounterState combatRandomEncounterState = Game.Instance.Player.CombatRandomEncounterState;
		EntityReference value;
		foreach (KeyValuePair<EntityReference, BlueprintUnit> spawner in combatRandomEncounterState.Spawners)
		{
			spawner.Deconstruct(out value, out var value2);
			EntityReference entityReference = value;
			BlueprintUnit blueprint = value2;
			UnitSpawner unitSpawner = entityReference.FindView() as UnitSpawner;
			if (unitSpawner == null)
			{
				PFLog.Default.Warning($"Cannot find view {unitSpawner} on scene or it hasn't type of UnitSpawner");
				continue;
			}
			unitSpawner.Blueprint = blueprint;
			unitSpawner.Spawn();
		}
		foreach (KeyValuePair<BlueprintAreaEnterPoint, EntityReference> allySpawnersInAllArea in combatRandomEncounterState.AllySpawnersInAllAreas)
		{
			allySpawnersInAllArea.Deconstruct(out var key, out value);
			BlueprintAreaEnterPoint blueprintAreaEnterPoint = key;
			EntityReference entityReference2 = value;
			if (combatRandomEncounterState.EnterPoint == blueprintAreaEnterPoint)
			{
				UnitSpawner unitSpawner2 = entityReference2.FindView() as UnitSpawner;
				if (!(unitSpawner2 == null))
				{
					unitSpawner2.Blueprint = combatRandomEncounterState.AllyBlueprint;
				}
			}
		}
		AreaRandomEncounter component = Game.Instance.CurrentlyLoadedArea.GetComponent<AreaRandomEncounter>();
		try
		{
			IEntityViewBase entityViewBase = combatRandomEncounterState.CoverGroup?.FindView();
			foreach (EntityReference item in component?.CoverGroupVariations.EmptyIfNull())
			{
				IEntityViewBase entityViewBase2 = item?.FindView();
				if (entityViewBase2 is MapObjectGroupView mapObjectGroupView)
				{
					mapObjectGroupView.Activate(entityViewBase2 == entityViewBase);
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Error(ex.Message);
		}
		try
		{
			IEntityViewBase entityViewBase3 = combatRandomEncounterState.TrapGroup?.FindView();
			foreach (EntityReference item2 in component?.TrapGroupVariations.EmptyIfNull())
			{
				IEntityViewBase entityViewBase4 = item2?.FindView();
				if (entityViewBase4 is TrapObjectGroupView trapObjectGroupView)
				{
					trapObjectGroupView.Activate(entityViewBase4 == entityViewBase3);
				}
			}
		}
		catch (Exception ex2)
		{
			PFLog.Default.Error(ex2.Message);
		}
		try
		{
			IEntityViewBase entityViewBase5 = combatRandomEncounterState.AreaEffectGroup?.FindView();
			foreach (EntityReference item3 in component?.AreaEffectGroupVariations.EmptyIfNull())
			{
				IEntityViewBase entityViewBase6 = item3?.FindView();
				if (entityViewBase6 is AreaEffectGroupView areaEffectGroupView)
				{
					areaEffectGroupView.Activate(entityViewBase6 == entityViewBase5);
				}
			}
		}
		catch (Exception ex3)
		{
			PFLog.Default.Error(ex3.Message);
		}
		try
		{
			IEntityViewBase entityViewBase7 = combatRandomEncounterState.OtherMapObjectGroup?.FindView();
			foreach (EntityReference item4 in component?.OtherMapObjectGroupVariations.EmptyIfNull())
			{
				IEntityViewBase entityViewBase8 = item4?.FindView();
				if (entityViewBase8 is MapObjectGroupView mapObjectGroupView2)
				{
					mapObjectGroupView2.Activate(entityViewBase8 == entityViewBase7);
				}
			}
		}
		catch (Exception ex4)
		{
			PFLog.Default.Error(ex4.Message);
		}
		combatRandomEncounterState.UnlockFlag?.Unlock();
		combatRandomEncounterState.GeneratedCombatRandomEncounterCount++;
		SetEnterPointsActive(active: false);
		EventBus.RaiseEvent(delegate(ICombatRandomEncounterHandler h)
		{
			h.HandleCombatRandomEncounterStart();
		});
	}

	private void SetEnterPointsActive(bool active)
	{
		foreach (MapObjectEntity item in Game.Instance.State.MapObjects.All.Where((MapObjectEntity entity) => entity.GetOptional<AreaTransitionPart>()?.AreaEnterPoint != null))
		{
			item.IsInGame = active;
		}
		m_IsEnterPointsActive = active;
	}

	public void OnEnable()
	{
		m_IsInRandomEncounter = Game.Instance.CurrentlyLoadedArea == Game.Instance.Player.CombatRandomEncounterState.Area;
	}

	public void OnDisable()
	{
	}
}
