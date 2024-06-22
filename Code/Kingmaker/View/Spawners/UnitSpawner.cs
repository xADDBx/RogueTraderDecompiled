using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("944f703c9407ac94aa1cc87bf43a3312")]
public class UnitSpawner : UnitSpawnerBase, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	private string m_GroupId;

	private string m_SquadId;

	private bool m_SquadLeader;

	private bool m_GroupIsDisabledOnSimplified;

	public bool IgnoreInEncoutnerStatistic;

	[SerializeField]
	private bool m_OverrideMusicCombatState;

	[SerializeField]
	[ShowIf("m_OverrideMusicCombatState")]
	private UnitVisualSettings.MusicCombatState m_MusicCombatState;

	[SerializeField]
	[Space(40f)]
	private bool m_BossMusicEnable;

	[SerializeField]
	[ShowIf("m_BossMusicEnable")]
	private AkStateReference m_MusicBossFightType;

	public UnitVisualSettings.MusicCombatState CombatMusic
	{
		get
		{
			if (!m_OverrideMusicCombatState)
			{
				return base.Blueprint.VisualSettings.CombatMusic;
			}
			return m_MusicCombatState;
		}
	}

	private bool IsExtra
	{
		get
		{
			if (TryGetComponent<SpawnerOptimizedUnit>(out var component))
			{
				return !component.IsLightweight;
			}
			return false;
		}
	}

	private bool IsLightweight
	{
		get
		{
			if (TryGetComponent<SpawnerOptimizedUnit>(out var component))
			{
				return component.IsLightweight;
			}
			return false;
		}
	}

	public void SetGroupView(UnitGroupView group)
	{
		m_GroupId = group.UniqueId;
		m_GroupIsDisabledOnSimplified = group.DisableOnSimplified;
	}

	public void SetSquadId(string id)
	{
		m_SquadId = id;
	}

	public void MarkAsSquadLeader()
	{
		m_SquadLeader = true;
	}

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		if (m_GroupIsDisabledOnSimplified)
		{
			base.HasSpawned = true;
			return null;
		}
		AbstractUnitEntity abstractUnitEntity = (IsLightweight ? Game.Instance.EntitySpawner.SpawnLightweightUnit(base.Blueprint, position, rotation, base.Data.HoldingState, base.SelectedCustomizationVariation) : Game.Instance.EntitySpawner.SpawnUnit(base.Blueprint, position, rotation, base.Data.HoldingState, base.SelectedCustomizationVariation));
		if (!(abstractUnitEntity is BaseUnitEntity baseUnitEntity))
		{
			return abstractUnitEntity;
		}
		if (!string.IsNullOrEmpty(m_GroupId))
		{
			baseUnitEntity.CombatGroup.Id = m_GroupId;
		}
		SetupSquad(baseUnitEntity);
		if (m_BossMusicEnable)
		{
			baseUnitEntity.MusicBossFightType = m_MusicBossFightType;
		}
		return baseUnitEntity;
	}

	private void SetupSquad([NotNull] BaseUnitEntity baseUnit)
	{
		if (!string.IsNullOrEmpty(m_SquadId))
		{
			PartSquad orCreate = baseUnit.GetOrCreate<PartSquad>();
			orCreate.Id = m_SquadId;
			if (m_SquadLeader)
			{
				orCreate.Squad.Leader = baseUnit;
			}
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity != null && abstractUnitEntity == base.SpawnedUnit)
		{
			CustomIdleAnimationMonoComponent component = GetComponent<CustomIdleAnimationMonoComponent>();
			if (!(component == null) && abstractUnitEntity.View.AnimationManager != null)
			{
				abstractUnitEntity.View.AnimationManager.CustomIdleWrappers = component.IdleClips;
			}
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (base.HasSpawned && abstractUnitEntity == base.SpawnedUnit)
		{
			base.Data.HasDied = true;
		}
	}

	public void HandleUnitDeath()
	{
	}

	protected override void OnInitialize(AbstractUnitEntity unit)
	{
		if (!unit.IsInCombat && !unit.IsDead && unit is BaseUnitEntity baseUnit)
		{
			SetupSquad(baseUnit);
		}
	}
}
