using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.Spawners;

public abstract class UnitSpawnerBase : EntityViewBase
{
	public class MyData : SimpleEntity, IHashable
	{
		[JsonProperty]
		private EntityRef<AbstractUnitEntity> m_SpawnedUnit;

		[JsonProperty]
		public bool HasSpawned { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public bool HasDied { get; set; }

		public new UnitSpawnerBase View => (UnitSpawnerBase)base.View;

		public EntityRef<AbstractUnitEntity> SpawnedUnit
		{
			get
			{
				return m_SpawnedUnit;
			}
			set
			{
				if (m_SpawnedUnit != value)
				{
					if (m_SpawnedUnit != null)
					{
						Clear();
					}
					m_SpawnedUnit = value;
					OnSpawned();
				}
			}
		}

		public override bool IsViewActive => true;

		public MyData(EntityViewBase view)
			: base(view)
		{
		}

		protected MyData(JsonConstructorMark _)
			: base(_)
		{
		}

		public virtual bool ShouldProcessActivation(bool alsoRaiseDead)
		{
			UnitSpawnerBase view = View;
			if (!view)
			{
				return false;
			}
			if (view.SpawnOnSceneInit)
			{
				if (view.HasSpawned && (!alsoRaiseDead || !view.m_RespawnIfDead || !view.SpawnedUnitHasDied))
				{
					if (HasSpawned)
					{
						return Game.Instance.Player.BrokenEntities.Contains(m_SpawnedUnit.Id);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected virtual void OnSpawned()
		{
			if (SpawnedUnit.Entity != null)
			{
				ApplyOnSpawn();
				HasSpawned = true;
			}
		}

		public void ApplyOnSpawn()
		{
			foreach (IUnitInitializer item in Parts.GetAll<IUnitInitializer>())
			{
				item.OnSpawn(SpawnedUnit);
				item.OnInitialize(SpawnedUnit);
			}
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			AbstractUnitEntity unit = SpawnedUnit.Entity;
			if (unit != null)
			{
				try
				{
					View.OnInitialize(unit);
				}
				catch (Exception exception)
				{
					PFLog.Default.ExceptionWithReport(exception, null);
				}
				Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
				{
					d.OnInitialize(unit);
				});
				if ((unit.SpawnPosition.To2D() - unit.Position.To2D()).magnitude < 0.1f)
				{
					AbstractUnitEntity abstractUnitEntity = unit;
					Vector3 spawnPosition = (unit.Position = View.ViewTransform.position);
					abstractUnitEntity.SpawnPosition = spawnPosition;
					unit.SetOrientation(View.ViewTransform.rotation.eulerAngles.y);
				}
			}
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return null;
		}

		protected override void OnDispose()
		{
			Clear();
			base.OnDispose();
		}

		public void Clear()
		{
			ApplyOnDispose();
			m_SpawnedUnit = default(EntityRef<AbstractUnitEntity>);
			HasSpawned = false;
		}

		public void ApplyOnDispose()
		{
			AbstractUnitEntity unit = SpawnedUnit.Entity;
			if (unit != null && !unit.WillBeDestroyed && !unit.Destroyed)
			{
				Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
				{
					d.OnDispose(unit);
				});
			}
		}

		protected override void OnIsInGameChanged()
		{
			base.OnIsInGameChanged();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			bool val2 = HasSpawned;
			result.Append(ref val2);
			bool val3 = HasDied;
			result.Append(ref val3);
			EntityRef<AbstractUnitEntity> obj = m_SpawnedUnit;
			Hash128 val4 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
			result.Append(ref val4);
			return result;
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Blueprint;

	[SerializeField]
	private bool m_SpawnOnSceneInit = true;

	[SerializeField]
	private bool m_RespawnIfDead;

	[SerializeField]
	private ConditionsReference m_spawnConditions;

	public bool HasSpawned
	{
		get
		{
			return Data.HasSpawned;
		}
		protected set
		{
			Data.HasSpawned = value;
		}
	}

	public BlueprintUnit Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	public AbstractUnitEntity SpawnedUnit
	{
		get
		{
			EntityRef<AbstractUnitEntity>? entityRef = Data?.SpawnedUnit;
			if (!entityRef.HasValue)
			{
				return null;
			}
			return entityRef.GetValueOrDefault();
		}
	}

	public bool SpawnOnSceneInit => m_SpawnOnSceneInit;

	public bool SpawnedUnitHasDied
	{
		get
		{
			if (HasSpawned)
			{
				return SpawnedUnit?.LifeState.IsDead ?? Data.HasDied;
			}
			return false;
		}
	}

	public new MyData Data => (MyData)base.Data;

	public override bool CreatesDataOnLoad => true;

	public virtual void HandleAreaSpawnerInit()
	{
		if ((!HasSpawned || (SpawnedUnitHasDied && m_RespawnIfDead)) && m_SpawnOnSceneInit && CheckConditions())
		{
			Game.Instance.EntityDestroyer.Destroy(Data.SpawnedUnit.Entity);
			Data.Clear();
			Spawn();
		}
		else if (HasSpawned && Game.Instance.Player.BrokenEntities.Contains(Data.SpawnedUnit.Id))
		{
			UberDebug.LogError("Respawning broken unit! {0}", Data.SpawnedUnit.Id);
			Game.Instance.Player.BrokenEntities.Remove(Data.SpawnedUnit.Id);
			ForceReSpawn();
		}
	}

	private bool CheckConditions()
	{
		if (m_spawnConditions.Get() != null)
		{
			ConditionsChecker conditions = m_spawnConditions.Get().Conditions;
			if (conditions.HasConditions)
			{
				return conditions.Check();
			}
			return true;
		}
		return true;
	}

	[CanBeNull]
	public AbstractUnitEntity Spawn()
	{
		if (HasSpawned)
		{
			PFLog.Default.Warning("Trying to use spawner {0} twice.", base.name);
			return null;
		}
		if (m_Blueprint.IsEmpty())
		{
			PFLog.Default.ErrorWithReport("UnitSpawnerBase.Spawn: unit blueprint is null! " + base.name);
			return null;
		}
		List<IUnitSpawnRestriction> list = TempList.Get<IUnitSpawnRestriction>();
		GetComponents(list);
		UnitSpawnRestrictionResult unitSpawnRestrictionResult = UnitSpawnRestrictionResult.CanSpawn;
		foreach (IUnitSpawnRestriction item in list)
		{
			UnitSpawnRestrictionResult unitSpawnRestrictionResult2 = item.CanSpawn();
			if (unitSpawnRestrictionResult2 > unitSpawnRestrictionResult)
			{
				unitSpawnRestrictionResult = unitSpawnRestrictionResult2;
			}
		}
		switch (unitSpawnRestrictionResult)
		{
		case UnitSpawnRestrictionResult.Delay:
			return null;
		case UnitSpawnRestrictionResult.Disable:
			HasSpawned = true;
			return null;
		default:
		{
			AbstractUnitEntity abstractUnitEntity = SpawnUnit(base.ViewTransform.position, base.ViewTransform.rotation);
			if (abstractUnitEntity == null)
			{
				return null;
			}
			Data.SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = base.ViewTransform.position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
			return abstractUnitEntity;
		}
		}
	}

	public void ForceReSpawn()
	{
		Game.Instance.EntityDestroyer.Destroy(Data.SpawnedUnit.Entity);
		Data.Clear();
		AbstractUnitEntity abstractUnitEntity = SpawnUnit(base.transform.position, base.transform.rotation);
		if (abstractUnitEntity != null)
		{
			Data.SpawnedUnit = abstractUnitEntity;
			abstractUnitEntity.SpawnPosition = base.transform.position;
			abstractUnitEntity.View.ForcePlaceAboveGround();
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MyData(this));
	}

	protected virtual AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		return null;
	}

	protected virtual void OnInitialize(AbstractUnitEntity unit)
	{
	}

	[NotNull]
	protected UnitViewLink SelectPrefab()
	{
		SpawnerCustomization component = GetComponent<SpawnerCustomization>();
		if ((bool)component && component.SelectedPrefab.Exists())
		{
			return component.SelectedPrefab;
		}
		if (Blueprint == null)
		{
			throw new Exception("UnitSpawner.SelectPrefab: missing blueprint");
		}
		if (Blueprint.Prefab == null)
		{
			throw new Exception("UnitSpawner.SelectPrefab: prefab is null");
		}
		return Blueprint.Prefab;
	}
}
