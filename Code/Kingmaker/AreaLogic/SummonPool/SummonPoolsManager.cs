using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.AreaLogic.SummonPool;

public class SummonPoolsManager : IService, IDisposable, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	private readonly Dictionary<BlueprintSummonPool, SummonPool> m_Pools = new Dictionary<BlueprintSummonPool, SummonPool>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public SummonPoolsManager()
	{
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
		m_Pools.Clear();
	}

	[CanBeNull]
	public ISummonPool Get([CanBeNull] BlueprintSummonPool blueprint)
	{
		if (blueprint == null)
		{
			return null;
		}
		return m_Pools.Get(blueprint);
	}

	public IEnumerable<BlueprintSummonPool> GetPoolsForUnit(AbstractUnitEntity unit)
	{
		foreach (KeyValuePair<BlueprintSummonPool, SummonPool> pool in m_Pools)
		{
			foreach (AbstractUnitEntity unit2 in pool.Value.Units)
			{
				if (unit2 == unit)
				{
					yield return pool.Key;
					break;
				}
			}
		}
	}

	private SummonPool GetOrCreateInternal([NotNull] BlueprintSummonPool blueprint)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
		SummonPool summonPool = m_Pools.Get(blueprint);
		if (summonPool == null)
		{
			summonPool = new SummonPool(blueprint);
			m_Pools.Add(blueprint, summonPool);
		}
		return summonPool;
	}

	public void Register(BlueprintSummonPool pool, AbstractUnitEntity unit)
	{
		GetOrCreateInternal(pool).Register(unit);
	}

	public void Unregister(BlueprintSummonPool pool, AbstractUnitEntity unit, bool keepReference = false)
	{
		GetOrCreateInternal(pool).Unregister(unit, keepReference);
	}

	[CanBeNull]
	public BaseUnitEntity Summon(BlueprintSummonPool poolBlueprint, BlueprintUnit unitBlueprint, Transform transform, Vector3 offset)
	{
		SummonPool orCreateInternal = GetOrCreateInternal(poolBlueprint);
		if (poolBlueprint.Limit > 0 && orCreateInternal.Count >= poolBlueprint.Limit)
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(unitBlueprint, transform.position + offset, transform.rotation, Game.Instance.State.LoadedAreaState.MainState);
		orCreateInternal.Register(baseUnitEntity);
		return baseUnitEntity;
	}

	public override string ToString()
	{
		return "SummonPoolsManager";
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		foreach (SummonPool item in m_Pools.Values.ToTempList())
		{
			item.Unregister(abstractUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		foreach (SummonPool item in m_Pools.Values.ToTempList())
		{
			if (!item.Blueprint.DoNotRemoveDeadUnits)
			{
				item.Unregister(abstractUnitEntity);
			}
		}
	}
}
