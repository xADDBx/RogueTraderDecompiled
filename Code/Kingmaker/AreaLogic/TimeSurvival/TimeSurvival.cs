using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[AllowedOn(typeof(BlueprintArea))]
[TypeId("22e6394dca698394997be7c45bbda818")]
public class TimeSurvival : EntityFactComponentDelegate, IRoundStartHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnEndHandler, ITurnEndHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int RoundsSurvived = -1;

		[JsonProperty]
		public Vector3 PreviousPlayerPosition;

		[JsonProperty]
		public bool IsEnded;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref RoundsSurvived);
			result.Append(ref PreviousPlayerPosition);
			result.Append(ref IsEnded);
			return result;
		}
	}

	[ValidateNotNull]
	[SerializeField]
	public List<SpawnData> spawnData;

	public bool UnlimitedTime;

	[HideIf("UnlimitedTime")]
	public int RoundsToSurvive = 5;

	public int RoundsPerSpawn = 2;

	public bool SpawnersShouldFollow = true;

	[SerializeField]
	private BlueprintBuffReference m_StartingBuff;

	public int m_BuffDuration = 1;

	private int m_FinalWaveRound;

	private bool m_FirstUnitTurnInterrupted;

	private List<BaseUnitEntity> m_UnitsSpawnedThisRound = new List<BaseUnitEntity>();

	private int m_LastUnitSpawnedIndex;

	private bool m_SpawningStage;

	private bool m_OutOfCombat;

	public BlueprintBuff StartingBuff => m_StartingBuff?.Get();

	public int RoundsLeft { get; private set; }

	public int CurrentRound { get; private set; }

	private Vector3 PlayerPosition => Game.Instance.Player.PlayerShip.Position;

	public void HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased)
		{
			if (m_OutOfCombat)
			{
				m_OutOfCombat = false;
			}
			else
			{
				StartSurvivalRound();
			}
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased && !CheckEnemiesAlive())
		{
			m_OutOfCombat = true;
			StartSurvivalRound();
		}
	}

	private bool CheckEnemiesAlive()
	{
		return !Game.Instance.State.AllUnits.Where((AbstractUnitEntity u) => u.IsPlayerEnemy && u.LifeState.IsConscious).Empty();
	}

	private void StartSurvivalRound()
	{
		m_FirstUnitTurnInterrupted = false;
		m_UnitsSpawnedThisRound.Clear();
		m_LastUnitSpawnedIndex = 0;
		SavableData savableData = RequestSavableData<SavableData>();
		if (!savableData.IsEnded)
		{
			IncrementRound(savableData);
			if (RoundsLeft <= 0)
			{
				FinishCombat(savableData);
			}
			if (!CheckEnemiesAlive() && CurrentRound > m_FinalWaveRound)
			{
				FinishCombat(savableData);
			}
		}
	}

	private void IncrementRound(SavableData data)
	{
		data.RoundsSurvived++;
		RoundsLeft = (UnlimitedTime ? 999 : (RoundsToSurvive - data.RoundsSurvived));
		CurrentRound = data.RoundsSurvived + 1;
		PFLog.Default.Log($"TimeSurvival: {CurrentRound} - {data.RoundsSurvived} of {RoundsToSurvive}");
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (ContextData<EventInvoker>.Current?.InvokerEntity is BaseUnitEntity)
		{
			InterruptTurnWithNewEnemy();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (m_FirstUnitTurnInterrupted)
		{
			return;
		}
		m_FirstUnitTurnInterrupted = true;
		SavableData data = RequestSavableData<SavableData>();
		if (data.IsEnded)
		{
			return;
		}
		if (!CheckEnemiesAlive())
		{
			while (!CanSpawnThisRound())
			{
				IncrementRound(data);
			}
		}
		if (CanSpawnThisRound())
		{
			Vector3 spawnShift = (SpawnersShouldFollow ? ((PlayerPosition - data.PreviousPlayerPosition) * 0.75f) : Vector3.zero);
			SpawnUnits(spawnShift);
			InterruptTurnWithNewEnemy();
		}
		if (SpawnersShouldFollow && data.RoundsSurvived > 0)
		{
			MoveSpawners(PlayerPosition, data.PreviousPlayerPosition);
		}
		data.PreviousPlayerPosition = PlayerPosition;
		bool CanSpawnThisRound()
		{
			if (data.RoundsSurvived != 0)
			{
				return data.RoundsSurvived % RoundsPerSpawn == 0;
			}
			return false;
		}
	}

	public bool IsShouldDoNothing(BaseUnitEntity unit)
	{
		if (CurrentRound == 1)
		{
			return false;
		}
		if (!m_SpawningStage)
		{
			return false;
		}
		if (m_UnitsSpawnedThisRound.Contains(unit))
		{
			return true;
		}
		return false;
	}

	private void FinishCombat(SavableData data)
	{
		foreach (UnitGroup item in Game.Instance.UnitGroups.Where((UnitGroup x) => !x.IsPlayerParty))
		{
			foreach (UnitReference unit in item.Units)
			{
				Game.Instance.EntityDestroyer.Destroy(unit.ToBaseUnitEntity());
			}
		}
		data.IsEnded = true;
		data.RoundsSurvived = -1;
		EventBus.RaiseEvent(delegate(IEndSpaceCombatHandler h)
		{
			h.HandleEndSpaceCombat();
		});
	}

	private List<SpawnData> ActiveSpawnData(IEnumerable<SpawnData> data, int currentRound)
	{
		return data.Where((SpawnData x) => x.RoundFrom <= currentRound && currentRound <= x.RoundTo).ToList();
	}

	private void SpawnUnits(Vector3 spawnShift)
	{
		m_SpawningStage = true;
		foreach (SpawnData item in ActiveSpawnData(spawnData, CurrentRound))
		{
			List<BlueprintUnit> unitsList = item.UnitsList;
			if (item.SpawnersList == null || item.SpawnersList.SpawnersPool.Count <= 0 || unitsList == null || unitsList.Count <= 0)
			{
				break;
			}
			HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
			using (PathDisposable<ShipPath> pathDisposable = Game.Instance.Player.PlayerShip?.Navigation.FindReachableTiles_Blocking(true))
			{
				ShipPath path = pathDisposable.Path;
				hashSet.AddRange(path.Result.Keys);
			}
			NavGraph graph = hashSet.First().Graph;
			List<EntityViewBase> list = new List<EntityViewBase>();
			foreach (EntityReference item2 in item.SpawnersList.SpawnersPool)
			{
				IEntityViewBase entityViewBase = item2.FindView();
				if (entityViewBase != null)
				{
					list.Add((EntityViewBase)entityViewBase);
				}
			}
			int num = 0;
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (num >= item.SpawnAttempts)
				{
					break;
				}
				int index = PFStatefulRandom.SpaceCombat.Range(0, list.Count);
				int index2 = PFStatefulRandom.SpaceCombat.Range(0, unitsList.Count);
				BlueprintUnit blueprintUnit = unitsList[index2];
				Transform viewTransform = list[index].ViewTransform;
				Vector3 position = viewTransform.position + spawnShift;
				GraphNode node = graph.GetNearest(position).node;
				list.RemoveAt(index);
				if (CanStandHere(node, SizePathfindingHelper.GetRectForSize(blueprintUnit.Size)))
				{
					BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(blueprintUnit, viewTransform.position + spawnShift, viewTransform.rotation, Game.Instance.State.LoadedAreaState.MainState);
					num++;
					m_UnitsSpawnedThisRound.Add(baseUnitEntity);
					if (StartingBuff != null)
					{
						BuffDuration duration = ((m_BuffDuration == 1) ? new BuffDuration(null, BuffEndCondition.TurnEndOrCombatEnd) : new BuffDuration(new Rounds(m_BuffDuration), BuffEndCondition.CombatEnd));
						baseUnitEntity.Buffs.Add(StartingBuff, duration);
					}
				}
			}
		}
	}

	private bool CanStandHere(GraphNode centerNode, IntRect sizeRect)
	{
		NodeList nodes = GridAreaHelper.GetNodes(centerNode, sizeRect);
		bool result = true;
		foreach (CustomGridNodeBase item in nodes)
		{
			if (item == null || !item.Walkable || WarhammerBlockManager.Instance.NodeContainsAny(item) || !UnitHelper.IsNodeConnected(item, nodes))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void MoveSpawners(Vector3 playerPosition, Vector3 prevPlayerPosition)
	{
		Vector3 vector = playerPosition - prevPlayerPosition;
		foreach (EntityReference item in spawnData.SelectMany((SpawnData x) => x.SpawnersList.SpawnersPool).ToList())
		{
			IEntityViewBase entityViewBase = item.FindView();
			if (entityViewBase != null)
			{
				entityViewBase.ViewTransform.position += vector;
			}
		}
	}

	private void InterruptTurnWithNewEnemy()
	{
		if (m_LastUnitSpawnedIndex >= m_UnitsSpawnedThisRound.Count)
		{
			m_SpawningStage = false;
			return;
		}
		BaseUnitEntity baseUnitEntity = m_UnitsSpawnedThisRound[m_LastUnitSpawnedIndex];
		m_LastUnitSpawnedIndex++;
		PartUnitCombatState combatStateOptional = baseUnitEntity.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			PFLog.Default.Error("Time Survival: can't interrupt turn, no Combat Part");
			return;
		}
		if (!combatStateOptional.IsInCombat)
		{
			combatStateOptional.JoinCombat();
		}
		Game.Instance.TurnController.InterruptCurrentTurn(baseUnitEntity, null, new InterruptionData
		{
			AsExtraTurn = true
		});
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<ITimeSurvivalSpawnHandler>)delegate(ITimeSurvivalSpawnHandler h)
		{
			h.HandleStarshipSpawnStarted();
		}, isCheckRuntime: true);
	}

	protected override void OnActivateOrPostLoad()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		RoundsLeft = RoundsToSurvive - savableData.RoundsSurvived;
		CurrentRound = savableData.RoundsSurvived + 1;
		m_FinalWaveRound = -1;
		foreach (SpawnData spawnDatum in spawnData)
		{
			m_FinalWaveRound = Mathf.Max(m_FinalWaveRound, spawnDatum.RoundTo);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
