using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.GuidUtility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("df167d2e283a452a9de40147e7751e4f")]
public class MakeSquadFromSummonPoolUnits : GameAction
{
	[SerializeField]
	[Tooltip("Юнит пул, из юнитов которого пробуем собрать отряд")]
	private BlueprintSummonPoolReference m_SummonPool;

	[SerializeField]
	[Tooltip("Пул в который хотим поместить лидера сквада")]
	private BlueprintSummonPoolReference m_SummonPoolForSquadLeader;

	[SerializeField]
	[Tooltip("Нужно проверять расстояние между юнитами, для формирования сквада")]
	private bool m_NeedCheckDistanceBetweenUnits;

	[SerializeField]
	[ShowIf("m_NeedCheckDistanceBetweenUnits")]
	[Tooltip("Максимальная дистанция")]
	private float m_MaxDistanceBetweenUnits = 1f;

	private BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	private BlueprintSummonPool SummonPoolForSquadLeader => m_SummonPoolForSquadLeader?.Get();

	public override string GetCaption()
	{
		return "Try to make a squad from a summon pool units";
	}

	protected override void RunAction()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool != null && summonPool.Count > 1)
		{
			if (m_NeedCheckDistanceBetweenUnits)
			{
				CreateSquadWithDistanceBetweenUnits(summonPool);
			}
			else
			{
				CreateSquadWithoutDistanceBetweenUnits(summonPool);
			}
		}
	}

	private void CreateSquadWithoutDistanceBetweenUnits(ISummonPool summonPool)
	{
		if (m_NeedCheckDistanceBetweenUnits)
		{
			return;
		}
		string squadId = Uuid.Instance.CreateString();
		bool squadLeader = true;
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			if (unit is BaseUnitEntity baseUnitEntity && baseUnitEntity.GetOptional<PartSquad>() == null)
			{
				SetupSquad(baseUnitEntity, squadId, squadLeader);
				squadLeader = false;
			}
		}
	}

	private void CreateSquadWithDistanceBetweenUnits(ISummonPool summonPool)
	{
		if (!m_NeedCheckDistanceBetweenUnits)
		{
			return;
		}
		string squadId = Uuid.Instance.CreateString();
		List<BaseUnitEntity> list = new List<BaseUnitEntity>(summonPool.Count);
		List<HashSet<int>> list2 = new List<HashSet<int>>(summonPool.Count);
		float num = m_MaxDistanceBetweenUnits * m_MaxDistanceBetweenUnits;
		int num2 = -1;
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			if (!(unit is BaseUnitEntity item) || list.Contains(item))
			{
				continue;
			}
			num2++;
			list.Add(item);
			list2.Add(new HashSet<int>());
			for (int i = 0; i < num2; i++)
			{
				if (!list2[num2].Contains(i) && (list[num2].Position - list[i].Position).sqrMagnitude <= num)
				{
					list2[num2].Add(i);
					list2[num2].UnionWith(list2[i]);
				}
			}
		}
		int num3 = -1;
		for (int j = 0; j < list2.Count; j++)
		{
			if (num3 == -1 && list2[j].Count >= 2)
			{
				num3 = j;
			}
			else if (list2[j].Count > list2[num3].Count)
			{
				num3 = j;
			}
		}
		if (num3 != -1)
		{
			SetupSquad(list[num3], squadId, squadLeader: true);
			foreach (int item2 in list2[num3])
			{
				SetupSquad(list[item2], squadId, squadLeader: false);
			}
		}
		list.Clear();
		list2.Clear();
	}

	private void SetupSquad([NotNull] BaseUnitEntity baseUnit, string squadId, bool squadLeader)
	{
		if (string.IsNullOrEmpty(squadId))
		{
			return;
		}
		PartSquad orCreate = baseUnit.GetOrCreate<PartSquad>();
		orCreate.Id = squadId;
		orCreate.SeparateUnitsAfterLeaderDeath = false;
		if (squadLeader)
		{
			orCreate.Squad.Leader = baseUnit;
			if (SummonPoolForSquadLeader != null)
			{
				Game.Instance.SummonPools.Register(SummonPoolForSquadLeader, baseUnit);
			}
		}
	}
}
