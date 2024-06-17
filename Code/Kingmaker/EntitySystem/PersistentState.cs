using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Settings;
using Kingmaker.SpaceCombat.MeteorStream;

namespace Kingmaker.EntitySystem;

public class PersistentState : IPersistentState, InterfaceService
{
	public readonly List<AreaPersistentState> SavedAreaStates = new List<AreaPersistentState>();

	public readonly EntityPool<Entity> Entities = new EntityPool<Entity>();

	public readonly EntityPool<Entity> SuppressibleEntities = new EntityPool<Entity>(CanBeSuppressed);

	public readonly EntityPool<Entity> EntitiesWithBounds = new EntityPool<Entity>(RequiresBounds);

	public readonly EntityPool<Entity> EntitiesAffectedByFogOfWar = new EntityPool<Entity>(AffectedByFogOfWar);

	public readonly EntityPool<MechanicEntity> MechanicEntities = new EntityPool<MechanicEntity>();

	public readonly EntityPool<AbstractUnitEntity> AllUnits = new EntityPool<AbstractUnitEntity>(NotFake);

	public readonly EntityPool<BaseUnitEntity> AllBaseUnits = new EntityPool<BaseUnitEntity>(NotFake);

	public readonly EntityPool<DestructibleEntity> DestructibleEntities = new EntityPool<DestructibleEntity>();

	public readonly EntityPool<MeteorStreamEntity> MeteorStreamEntities = new EntityPool<MeteorStreamEntity>();

	public readonly EntityPool<CutscenePlayerData> Cutscenes = new EntityPool<CutscenePlayerData>();

	public readonly EntityPool<MapObjectEntity> MapObjects = new EntityPool<MapObjectEntity>();

	public readonly EntityPool<AreaEffectEntity> AreaEffects = new EntityPool<AreaEffectEntity>();

	public readonly EntityPool<ScriptZoneEntity> ScriptZones = new EntityPool<ScriptZoneEntity>();

	public readonly EntityPool<StarSystemObjectEntity> StarSystemObjects = new EntityPool<StarSystemObjectEntity>();

	public readonly EntityPool<SectorMapObjectEntity> SectorMapObjects = new EntityPool<SectorMapObjectEntity>();

	public readonly EntityPool<SectorMapRumourEntity> SectorMapRumours = new EntityPool<SectorMapRumourEntity>();

	public readonly List<AbstractUnitEntity> AllAwakeUnits = new List<AbstractUnitEntity>();

	public readonly List<BaseUnitEntity> AllBaseAwakeUnits = new List<BaseUnitEntity>();

	public Player PlayerState = Entity.Initialize(new Player());

	public InGameSettings InGameSettings = new InGameSettings();

	public CoopData CoopData = new CoopData();

	public AreaPersistentState LoadedAreaState { get; set; }

	public AreaPersistentState GetStateForArea(BlueprintArea area)
	{
		AreaPersistentState areaPersistentState = SavedAreaStates.SingleOrDefault((AreaPersistentState s) => s.Blueprint == area);
		if (areaPersistentState == null)
		{
			SavedAreaStates.Add(areaPersistentState = new AreaPersistentState(area));
		}
		return areaPersistentState;
	}

	public void SetNewAwakeUnits(IEnumerable<AbstractUnitEntity> awakeUnits)
	{
		ClearAwakeUnits();
		foreach (AbstractUnitEntity awakeUnit in awakeUnits)
		{
			if (awakeUnit is BaseUnitEntity item)
			{
				AllBaseAwakeUnits.Add(item);
			}
			AllAwakeUnits.Add(awakeUnit);
		}
	}

	public void ClearAwakeUnits()
	{
		AllAwakeUnits.Clear();
		AllBaseAwakeUnits.Clear();
	}

	private static bool NotFake(AbstractUnitEntity unit)
	{
		return !unit.Blueprint.IsFake;
	}

	private static bool IsExtra(AbstractUnitEntity unit)
	{
		if (NotFake(unit))
		{
			return unit.IsExtra;
		}
		return false;
	}

	private static bool CanBeSuppressed(Entity entity)
	{
		return entity.IsSuppressible;
	}

	private static bool RequiresBounds(Entity entity)
	{
		return entity.GetOptional<EntityBoundsPart>() != null;
	}

	private static bool AffectedByFogOfWar(Entity entity)
	{
		return entity.IsAffectedByFogOfWar;
	}

	public IEntity GetEntityDataFromLoadedAreaState(string uniqueID)
	{
		return LoadedAreaState.AllEntityData.SingleOrDefault((Entity e) => e.UniqueId == uniqueID);
	}
}
