using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Parts.Crew;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipPartCrew : StarshipPart, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipPartCrew>, IEntityPartOwner
	{
	}

	private class ModuleComparer : Comparer<StarshipModuleCrewWrapper>
	{
		public override int Compare(StarshipModuleCrewWrapper x, StarshipModuleCrewWrapper y)
		{
			if (x.GetRatio(includeTransitionCrew: true) > y.GetRatio(includeTransitionCrew: true))
			{
				return 1;
			}
			if (x.GetRatio(includeTransitionCrew: true) < y.GetRatio(includeTransitionCrew: true))
			{
				return -1;
			}
			if (x.Max > y.Max)
			{
				return -1;
			}
			if (x.Max >= y.Max)
			{
				return 0;
			}
			return 1;
		}
	}

	[JsonProperty]
	private Dictionary<ShipModuleType, StarshipModuleCrewData> m_Data;

	[JsonProperty]
	public int HandledCrewLoss;

	private const int DefaultDistributeCountPerIteration = 1;

	private readonly Dictionary<ShipModuleType, StarshipModuleCrewWrapper> m_CrewData = new Dictionary<ShipModuleType, StarshipModuleCrewWrapper>();

	private readonly List<StarshipModuleCrewWrapper> m_ModulesWithoutQuarters = new List<StarshipModuleCrewWrapper>();

	private StarshipModuleCrewWrapper m_Quarters;

	private StarshipCrewPenalties m_CrewPenalties;

	private readonly ModuleComparer m_ModuleComparer = new ModuleComparer();

	public int MaxCount { get; private set; }

	public int Count { get; private set; }

	public float Ratio { get; private set; }

	private int Throughput => base.Owner.Blueprint.CrewQuartersThroughput;

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		m_CrewPenalties = base.Blueprint.GetComponent<StarshipCrewPenalties>();
		if (m_Data == null)
		{
			m_Data = new Dictionary<ShipModuleType, StarshipModuleCrewData>();
		}
		foreach (ShipModuleSettings module in base.Owner.Blueprint.Modules)
		{
			if (!m_Data.ContainsKey(module.ModuleType))
			{
				m_Data.Add(module.ModuleType, new StarshipModuleCrewData());
			}
			StarshipModuleCrewWrapper starshipModuleCrewWrapper = Wrap(m_Data[module.ModuleType], module);
			m_CrewData.Add(module.ModuleType, starshipModuleCrewWrapper);
			if (module.ModuleType == ShipModuleType.CrewQuarters)
			{
				m_Quarters = starshipModuleCrewWrapper;
			}
			else
			{
				m_ModulesWithoutQuarters.Add(starshipModuleCrewWrapper);
			}
			MaxCount += starshipModuleCrewWrapper.Max;
			Count += starshipModuleCrewWrapper.GetAliveCount(includeInTransition: true);
		}
		if (MaxCount != 0)
		{
			Ratio = (float)Count / (float)MaxCount;
		}
		StarshipModuleCrewWrapper Wrap(StarshipModuleCrewData data, ShipModuleSettings x)
		{
			return new StarshipModuleCrewWrapper(base.Owner, data, x, delegate
			{
				TryPerformActionsOnCrewLoss();
				RecalculateCrewCount();
			}, RecalculateCrewCount);
		}
	}

	public bool CanDistributeQuarters()
	{
		if (!m_Quarters.IsFull(includeTransitionCrew: true))
		{
			return m_ModulesWithoutQuarters.Any((StarshipModuleCrewWrapper x) => !x.IsFull(includeTransitionCrew: true));
		}
		return false;
	}

	public IReadOnlyStarshipModuleCrewWrapper GetReadOnlyCrewData(ShipModuleType moduleType)
	{
		return m_CrewData[moduleType];
	}

	public void Damage(int damage)
	{
		foreach (StarshipModuleCrewWrapper value in m_CrewData.Values)
		{
			value.Remove(damage, isDamage: true);
		}
	}

	public void Heal(bool healAll, int amount)
	{
		foreach (StarshipModuleCrewWrapper value in m_CrewData.Values)
		{
			value.Add(healAll ? value.Data.CountLost : amount, isHeal: true);
		}
	}

	private void RecalculateCrewCount()
	{
		if (MaxCount != 0)
		{
			Count = m_CrewData.Sum((KeyValuePair<ShipModuleType, StarshipModuleCrewWrapper> x) => x.Value.GetAliveCount(includeInTransition: true));
			Ratio = (float)Count / (float)MaxCount;
		}
	}

	public void RestoreCrew()
	{
		HandledCrewLoss = 0;
		foreach (StarshipModuleCrewWrapper value in m_CrewData.Values)
		{
			value.Restore();
		}
	}

	private void TryPerformActionsOnCrewLoss()
	{
		if (m_CrewPenalties != null)
		{
			int num = m_CrewData.Sum((KeyValuePair<ShipModuleType, StarshipModuleCrewWrapper> x) => x.Value.Data.CountLostOnCurrentTurn);
			int crewDamagePerRoundThreshold = m_CrewPenalties.CrewDamagePerRoundThreshold;
			while (num >= HandledCrewLoss + crewDamagePerRoundThreshold)
			{
				HandledCrewLoss += crewDamagePerRoundThreshold;
				m_CrewPenalties.Penalties.Run();
			}
		}
	}

	public void MoveCrewToQuarters(ShipModuleType fromModule, int count)
	{
		StarshipModuleCrewWrapper starshipModuleCrewWrapper = m_CrewData[fromModule];
		int val = Math.Min(starshipModuleCrewWrapper.GetAliveCount(includeInTransition: false), count);
		val = Math.Min(val, m_Quarters.FreePlace(includeTransitionCrew: true));
		starshipModuleCrewWrapper.Remove(val, isDamage: false);
		m_Quarters.Add(val, isHeal: false);
	}

	public void DistributeQuarters(int availableToDistributeCount)
	{
		availableToDistributeCount = Math.Min(availableToDistributeCount, m_Quarters.GetAliveCount(includeInTransition: false));
		while (availableToDistributeCount > 0)
		{
			m_ModulesWithoutQuarters.Sort(m_ModuleComparer);
			StarshipModuleCrewWrapper starshipModuleCrewWrapper = m_ModulesWithoutQuarters[0];
			if (!starshipModuleCrewWrapper.IsFull(includeTransitionCrew: true))
			{
				float ratio = starshipModuleCrewWrapper.GetRatio(includeTransitionCrew: true);
				float ratio2 = m_ModulesWithoutQuarters[1].GetRatio(includeTransitionCrew: true);
				int num = 1;
				if (ratio2 > ratio)
				{
					num = (int)Math.Ceiling(ratio2 * (float)starshipModuleCrewWrapper.Max) - starshipModuleCrewWrapper.GetAliveCount(includeInTransition: true);
					num = Math.Min(num, availableToDistributeCount);
				}
				starshipModuleCrewWrapper.StartTransit(num);
				m_Quarters.Remove(num, isDamage: false);
				availableToDistributeCount -= num;
				continue;
			}
			break;
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<ShipModuleType, StarshipModuleCrewData> data = m_Data;
		if (data != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<ShipModuleType, StarshipModuleCrewData> item in data)
			{
				Hash128 hash = default(Hash128);
				ShipModuleType obj = item.Key;
				Hash128 val3 = UnmanagedHasher<ShipModuleType>.GetHash128(ref obj);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<StarshipModuleCrewData>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		result.Append(ref HandledCrewLoss);
		return result;
	}
}
