using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceManagement;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.MapObjects.Traps.Detailed;
using Kingmaker.Visual.Sound;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Controllers;

public class EntitySpawnController : IControllerTick, IController, IDisposable
{
	public struct SpawnEntry
	{
		public Entity Entity;

		public SceneEntitiesState State;
	}

	public class EntitySpawnData : ContextData<EntitySpawnData>
	{
		public EntityViewBase View { get; private set; }

		public SceneEntitiesState TargetState { get; private set; }

		public EntitySpawnData Setup(EntityViewBase view, SceneEntitiesState state)
		{
			View = view;
			TargetState = state;
			return this;
		}

		protected override void Reset()
		{
			View = null;
			TargetState = null;
		}
	}

	private readonly List<SpawnEntry> m_ToSpawn = new List<SpawnEntry>();

	public readonly CountableFlag SuppressSpawn = new CountableFlag();

	[NotNull]
	public IEnumerable<SpawnEntry> CreationQueue => m_ToSpawn;

	public T SpawnEntityWithView<T>(T prefab, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state) where T : EntityViewBase
	{
		return (T)SpawnEntityWithView((EntityViewBase)prefab, position, rotation, state).View;
	}

	private Entity SpawnEntityWithView(EntityViewBase prefab, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state)
	{
		EntityViewBase entityViewBase = UnityEngine.Object.Instantiate(prefab, position, rotation);
		entityViewBase.UniqueId = Uuid.Instance.CreateString();
		return SpawnEntityWithView(entityViewBase, state);
	}

	public Entity SpawnEntityWithView(EntityViewBase view, [CanBeNull] SceneEntitiesState state, bool moveView = true)
	{
		if ((bool)SuppressSpawn)
		{
			throw new Exception("Попытка создать новую сущность при заблокированном EntitySpawnController. Вероятно, вы пытаетесь сделать этот в неправильный момент");
		}
		if (state == null)
		{
			state = Game.Instance.State.LoadedAreaState.MainState;
		}
		using (ContextData<EntitySpawnData>.Request().Setup(view, state))
		{
			Entity entity = view.CreateEntityData(load: false);
			entity.AttachView(view);
			if (moveView)
			{
				SpawnEntity(entity, state);
			}
			else
			{
				SpawnEntityWithoutMovingView(entity, state);
			}
			if (view is UnitEntityView unitEntityView)
			{
				unitEntityView.ForcePlaceAboveGround();
			}
			return entity;
		}
	}

	private void SpawnEntityWithoutMovingView([NotNull] Entity entity, [CanBeNull] SceneEntitiesState state)
	{
		m_ToSpawn.Add(new SpawnEntry
		{
			Entity = entity,
			State = state
		});
	}

	public void SpawnEntity([NotNull] Entity entity, [CanBeNull] SceneEntitiesState state)
	{
		if (state == null)
		{
			state = Game.Instance.State.LoadedAreaState.MainState;
		}
		if (entity.View != null)
		{
			((state == Game.Instance.Player.CrossSceneState) ? Game.Instance.CrossSceneRoot : Game.Instance.DynamicRoot).Add(entity.View.ViewTransform);
		}
		m_ToSpawn.Add(new SpawnEntry
		{
			Entity = entity,
			State = state
		});
	}

	public BaseUnitEntity SpawnUnit(BlueprintUnit unit, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state)
	{
		return SpawnUnit(unit, position, rotation, state, null);
	}

	public BaseUnitEntity SpawnUnit(BlueprintUnit unit, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state, [CanBeNull] UnitCustomizationVariation variation)
	{
		if (unit == null)
		{
			PFLog.Default.Error("Trying to spawn null unit");
			return null;
		}
		if (BuildModeUtility.Data.Loading.IgnoreSpawners)
		{
			PFLog.Default.Error("Skip spawn of " + unit);
			return null;
		}
		if (unit.CustomizationPreset != null)
		{
			if (variation == null || string.IsNullOrEmpty(variation.Prefab.AssetId))
			{
				variation = unit.CustomizationPreset.SelectVariation(unit, null);
			}
			if (variation != null)
			{
				if (variation.Prefab.AssetId.IsNullOrEmpty())
				{
					PFLog.Default.Error("This unit {0} not contain a variation prefab (preset = {1}, need put variation prefab)", unit, unit.CustomizationPreset);
					using BundledResourceHandle<UnitEntityView> bundledResourceHandle = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId);
					return SpawnUnit(unit, bundledResourceHandle.Object, position, rotation, state);
				}
				BlueprintUnitAsksList voice = unit.CustomizationPreset.SelectVoice(variation.Gender);
				using (variation.CreateSpawningData(voice))
				{
					using BundledResourceHandle<UnitEntityView> bundledResourceHandle2 = BundledResourceHandle<UnitEntityView>.Request(variation.Prefab.AssetId);
					return SpawnUnit(unit, bundledResourceHandle2.Object, position, rotation, state);
				}
			}
			PFLog.Default.Warning("This unit {0} should not contain a customization preset (preset = {1}, need clean the CustomizationPreset field in his BlueprintUnit)", unit, unit.CustomizationPreset);
		}
		using BundledResourceHandle<UnitEntityView> bundledResourceHandle3 = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId);
		return SpawnUnit(unit, bundledResourceHandle3.Object, position, rotation, state);
	}

	public BaseUnitEntity SpawnUnit(BlueprintUnit unit, UnitEntityView prefab, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state)
	{
		UnitEntityView unitEntityView = null;
		try
		{
			if (unit == null)
			{
				PFLog.Default.Error("Trying to spawn null unit");
				return null;
			}
			if (prefab == null)
			{
				PFLog.Default.Error("Trying to spawn unit without prefab {0}", unit);
				return null;
			}
			unitEntityView = UnityEngine.Object.Instantiate(prefab, position, rotation);
			unitEntityView.UniqueId = Uuid.Instance.CreateString();
			unitEntityView.Blueprint = unit;
			return (BaseUnitEntity)SpawnEntityWithView(unitEntityView, state);
		}
		catch (Exception ex)
		{
			if (unitEntityView != null)
			{
				UnityEngine.Object.Destroy(unitEntityView.gameObject);
			}
			PFLog.Default.Exception(ex);
			return null;
		}
	}

	public AbstractUnitEntity SpawnLightweightUnit(BlueprintUnit unit, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state, [CanBeNull] UnitCustomizationVariation variation)
	{
		if (unit == null)
		{
			PFLog.Default.Error("Trying to spawn null unit");
			return null;
		}
		if (BuildModeUtility.Data.Loading.IgnoreSpawners)
		{
			PFLog.Default.Error("Skip spawn of " + unit);
			return null;
		}
		if (unit.CustomizationPreset != null)
		{
			if (variation == null || string.IsNullOrEmpty(variation.Prefab.AssetId))
			{
				variation = unit.CustomizationPreset.SelectVariation(unit, null);
			}
			if (variation != null)
			{
				if (variation.Prefab.AssetId.IsNullOrEmpty())
				{
					PFLog.Default.Error("This unit {0} not contain a variation prefab (preset = {1}, need put variation prefab)", unit, unit.CustomizationPreset);
					using BundledResourceHandle<UnitEntityView> bundledResourceHandle = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId);
					return SpawnLightweightUnit(unit, bundledResourceHandle.Object, position, rotation, state);
				}
				BlueprintUnitAsksList voice = unit.CustomizationPreset.SelectVoice(variation.Gender);
				using (variation.CreateSpawningData(voice))
				{
					using BundledResourceHandle<UnitEntityView> bundledResourceHandle2 = BundledResourceHandle<UnitEntityView>.Request(variation.Prefab.AssetId);
					return SpawnLightweightUnit(unit, bundledResourceHandle2.Object, position, rotation, state);
				}
			}
			PFLog.Default.Warning("This unit {0} should not contain a customization preset (preset = {1}, need clean the CustomizationPreset field in his BlueprintUnit)", unit, unit.CustomizationPreset);
		}
		using BundledResourceHandle<UnitEntityView> bundledResourceHandle3 = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId);
		return SpawnLightweightUnit(unit, bundledResourceHandle3.Object, position, rotation, state);
	}

	public AbstractUnitEntity SpawnLightweightUnit(BlueprintUnit unit, UnitEntityView prefab, Vector3 position, Quaternion rotation, [CanBeNull] SceneEntitiesState state)
	{
		if (unit == null)
		{
			PFLog.Default.Error("Trying to spawn null unit");
			return null;
		}
		if (prefab == null)
		{
			PFLog.Default.Error("Trying to spawn unit without prefab {0}", unit);
			return null;
		}
		LightweightUnitEntity lightweightUnitEntity = Entity.Initialize(new LightweightUnitEntity(Uuid.Instance.CreateString(), isInGame: true, unit));
		lightweightUnitEntity.CreateView(prefab, position, rotation);
		lightweightUnitEntity.Position = position;
		lightweightUnitEntity.SetOrientation(rotation.eulerAngles.y);
		SpawnEntity(lightweightUnitEntity, state);
		return lightweightUnitEntity;
	}

	public DetailedTrapObjectData SpawnTrap(BlueprintTrap trap, ScriptZone scriptZone, SceneEntitiesState state, Guid? uniqueId = null)
	{
		DetailedTrapObjectView view = DetailedTrapObjectView.CreateView(trap, uniqueId.ToString(), scriptZone.UniqueId);
		DetailedTrapObjectData obj = (DetailedTrapObjectData)SpawnEntityWithView(view, state);
		obj.View.OnAreaDidLoad();
		return obj;
	}

	public DynamicMapObjectView.EntityData SpawnMapObject(BlueprintDynamicMapObject blueprint, Vector3 position, Quaternion rotation, SceneEntitiesState state)
	{
		if (blueprint.Prefab == null)
		{
			PFLog.Default.Error("Trying to spawn map object without prefab: {0}", blueprint);
			return null;
		}
		DynamicMapObjectView component = blueprint.Prefab.GetComponent<DynamicMapObjectView>();
		component.Blueprint = blueprint;
		return (DynamicMapObjectView.EntityData)SpawnEntityWithView(component, position, rotation, state).Data;
	}

	public BaseUnitEntity ChangeUnitBlueprint(BaseUnitEntity unit, BlueprintUnit newBlueprint, bool toCrossState, bool keepOld = false)
	{
		UnitEntityView view = unit.View;
		if (!keepOld)
		{
			Game.Instance.EntityDestroyer.Destroy(unit);
		}
		else
		{
			unit.IsInGame = false;
		}
		unit.DetachView();
		view.Blueprint = newBlueprint;
		view.UniqueId = Uuid.Instance.CreateString();
		UnitEntity unitEntity = Entity.Initialize(new UnitEntity(view));
		foreach (WeaponSet value in view.HandsEquipment.Sets.Values)
		{
			value.MainHand.DestroyModel();
			value.OffHand.DestroyModel();
		}
		unitEntity.AttachView(view);
		unitEntity.Position = unit.Position;
		unitEntity.SetOrientation(unit.Orientation);
		SpawnEntry spawnEntry = default(SpawnEntry);
		spawnEntry.Entity = unitEntity;
		spawnEntry.State = (toCrossState ? Game.Instance.Player.CrossSceneState : Game.Instance.State.LoadedAreaState.MainState);
		SpawnEntry item = spawnEntry;
		m_ToSpawn.Add(item);
		return unitEntity;
	}

	public BaseUnitEntity RecruitNPC([CanBeNull] BaseUnitEntity npc, BlueprintUnit companionBlueprint)
	{
		BaseUnitEntity baseUnitEntity = UnitPartCompanion.FindCompanion(companionBlueprint, CompanionState.ExCompanion);
		if (baseUnitEntity == null)
		{
			baseUnitEntity = ChangeUnitBlueprint(npc, companionBlueprint, toCrossState: true);
		}
		else
		{
			if (npc != null && npc != baseUnitEntity)
			{
				Game.Instance.EntityDestroyer.Destroy(npc);
				baseUnitEntity.Position = npc.Position;
				baseUnitEntity.SetOrientation(npc.Orientation);
			}
			baseUnitEntity.IsInGame = true;
		}
		if (!baseUnitEntity.IsPet)
		{
			Game.Instance.Player.AddCompanion(baseUnitEntity);
		}
		return baseUnitEntity;
	}

	public BaseUnitEntity UnrecruitNPC(BaseUnitEntity npc, BlueprintUnit companionBlueprint)
	{
		Game.Instance.Player.RemoveCompanion(npc, stayInGame: true);
		return ChangeUnitBlueprint(npc, companionBlueprint, toCrossState: false);
	}

	public bool IsViewWaitToCreate(EntityViewBase view)
	{
		foreach (SpawnEntry item in m_ToSpawn)
		{
			if (item.Entity == view.Data)
			{
				if (item.State.IsSceneLoaded)
				{
					return true;
				}
				throw new Exception($"View {view} can't not be created, becouse state {item.State} not loaded");
			}
		}
		return false;
	}

	public void SpawnEntityImmediately([NotNull] Entity entity, [NotNull] SceneEntitiesState state)
	{
		state.AddEntityData(entity);
		if (state == Game.Instance.Player.CrossSceneState)
		{
			entity.GetOrCreate<PartHoldPrefabBundle>();
		}
		(entity as MechanicEntity)?.HandleSpawn();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		for (int i = 0; i < m_ToSpawn.Count; i++)
		{
			SpawnEntry spawnEntry = m_ToSpawn[i];
			SpawnEntityImmediately(spawnEntry.Entity, spawnEntry.State);
		}
		m_ToSpawn.Clear();
	}

	public void Dispose()
	{
		m_ToSpawn.ForEach(delegate(SpawnEntry e)
		{
			e.Entity.Dispose();
		});
		m_ToSpawn.Clear();
	}

	public bool TryRemoveFromSpawnQueue(Entity entity)
	{
		return m_ToSpawn.RemoveAll((SpawnEntry i) => i.Entity == entity) > 0;
	}
}
