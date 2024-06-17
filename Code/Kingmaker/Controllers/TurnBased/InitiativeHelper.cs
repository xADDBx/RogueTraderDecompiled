using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public static class InitiativeHelper
{
	private enum InitiativeType
	{
		Squad,
		SquadLeader,
		SquadMember,
		Independent,
		Multi,
		Other
	}

	public static void Roll(IEnumerable<MechanicEntity> newCombatants, bool relax)
	{
		relax &= !EtudeBracketForceInitiativeOrder.Any;
		if (!Game.Instance.TurnController.TbActive || newCombatants.Empty())
		{
			return;
		}
		List<MechanicEntity> list = newCombatants.Where((MechanicEntity i) => i.Initiative.Empty).ToTempList();
		list.Sort(OrderEntitiesByInitiativeType);
		foreach (MechanicEntity item in list)
		{
			RollInitiative(item);
		}
		if (relax)
		{
			RelaxInitiativeRolls(list);
		}
		TryForceInitiativeOrder(list);
		foreach (MechanicEntity item2 in list)
		{
			ApplyInitiative(item2);
		}
		PostProcessInitiative(list);
		foreach (MechanicEntity item3 in list)
		{
			UpdateBuffsInitiative(item3);
		}
	}

	public static void Update()
	{
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			UpdateBuffsInitiative(mechanicEntity);
		}
		foreach (MechanicEntity mechanicEntity2 in Game.Instance.State.MechanicEntities)
		{
			if (!Game.Instance.TurnController.TbActive)
			{
				mechanicEntity2.Initiative.Clear();
			}
			else if (mechanicEntity2 is AreaEffectEntity areaEffectEntity)
			{
				areaEffectEntity.UpdateCombatInitiative();
			}
		}
	}

	private static void RollInitiative(MechanicEntity entity)
	{
		if (entity.GetInitiativeRollProvider() == entity)
		{
			float? num = entity.GetCombatStateOptional()?.OverrideInitiative;
			if (num.HasValue)
			{
				float valueOrDefault = num.GetValueOrDefault();
				entity.Initiative.Roll = Math.Max(1f, valueOrDefault);
			}
			else
			{
				MechanicEntity initiator = ((entity is UnitSquad unitSquad) ? unitSquad.InitiativeRoller : entity);
				entity.Initiative.Roll = Math.Max(1f, Rulebook.Trigger(new RuleRollInitiative(initiator)).Result);
			}
		}
	}

	private static void RelaxInitiativeRolls(IEnumerable<MechanicEntity> entities)
	{
		InitiativeDistribution random = InitiativeDistribution.GetRandom();
		InitiativeDistribution.Range[] array = random?.Ranges;
		if (array != null && array.Length > 0)
		{
			IEnumerable<MechanicEntity> enumerable = CollectInitiativeSubjects(entities);
			if (!enumerable.Empty())
			{
				Func<float> relaxedInitiativeRollIterator = GetRelaxedInitiativeRollIterator(enumerable);
				RelaxInitiativeRolls(random, enumerable, relaxedInitiativeRollIterator);
			}
		}
	}

	private static void RelaxInitiativeRolls(InitiativeDistribution distribution, IEnumerable<MechanicEntity> subjects, Func<float> getNextInitiativeRoll)
	{
		(IEnumerable<MechanicEntity> player, IEnumerable<MechanicEntity> npc) tuple = SplitByFaction(subjects);
		List<MechanicEntity> list = tuple.player.OrderBy((MechanicEntity i) => i.Initiative.Roll).ToTempList();
		List<MechanicEntity> list2 = tuple.npc.OrderBy((MechanicEntity i) => i.Initiative.Roll).ToTempList();
		List<MechanicEntity> list4;
		List<MechanicEntity> list5;
		if (!distribution.StartsFromPlayer)
		{
			List<MechanicEntity> list3 = list;
			list4 = list2;
			list5 = list3;
		}
		else
		{
			List<MechanicEntity> list3 = list2;
			list4 = list;
			list5 = list3;
		}
		InitiativeDistribution.Range[] ranges = distribution.Ranges;
		foreach (InitiativeDistribution.Range range in ranges)
		{
			int num = PFStatefulRandom.Mechanics.Range(Math.Max(1, range.Min), Math.Max(1, range.Max) + 1);
			MechanicEntity value;
			while (num-- > 0 && list4.TryPop(out value))
			{
				value.Initiative.Roll = getNextInitiativeRoll();
			}
			if (list5.Empty())
			{
				break;
			}
			List<MechanicEntity> list6 = list5;
			List<MechanicEntity> list3 = list4;
			list4 = list6;
			list5 = list3;
		}
		MechanicEntity value2;
		while (list4.TryPop(out value2))
		{
			value2.Initiative.Roll = getNextInitiativeRoll();
			if (!list5.Empty())
			{
				List<MechanicEntity> list7 = list5;
				List<MechanicEntity> list3 = list4;
				list4 = list7;
				list5 = list3;
			}
		}
	}

	private static IEnumerable<MechanicEntity> CollectInitiativeSubjects(IEnumerable<MechanicEntity> entities)
	{
		return entities.Where((MechanicEntity i) => i.GetInitiativeRollProvider() == i);
	}

	private static Func<float> GetRelaxedInitiativeRollIterator(IEnumerable<MechanicEntity> subjects)
	{
		int num = subjects.Count();
		float num2 = Math.Max(1f, subjects.Min((MechanicEntity i) => i.Initiative.Roll));
		float num3 = Math.Max(1f, subjects.Max((MechanicEntity i) => i.Initiative.Roll));
		float step = Math.Min(1f, (num3 - num2) / (float)num);
		float current = num2 + step * (float)num;
		return delegate
		{
			float result = Math.Max(1f, current);
			current -= step;
			return result;
		};
	}

	private static (IEnumerable<MechanicEntity> player, IEnumerable<MechanicEntity> npc) SplitByFaction(IEnumerable<MechanicEntity> subjects)
	{
		return (player: subjects.Where((MechanicEntity i) => i.IsInPlayerParty), npc: subjects.Where((MechanicEntity i) => !i.IsInPlayerParty));
	}

	private static void TryForceInitiativeOrder(List<MechanicEntity> entities)
	{
		IEnumerable<MechanicEntity> firstOrder = GetFirstOrder(entities);
		IEnumerable<MechanicEntity> lastOrder = GetLastOrder(entities);
		if (firstOrder.Empty() && lastOrder.Empty())
		{
			return;
		}
		foreach (MechanicEntity entity in entities)
		{
			int num = firstOrder.IndexOf(entity);
			if (num != -1)
			{
				entity.Initiative.Roll = 15762925 - num * 10;
				continue;
			}
			num = lastOrder.IndexOf(entity);
			if (num != -1)
			{
				entity.Initiative.Roll = -(num * 10);
			}
		}
	}

	private static IEnumerable<MechanicEntity> GetFirstOrder(List<MechanicEntity> entities)
	{
		if (EtudeBracketForceInitiativeOrder.GetOrderOptional() == null)
		{
			return entities.Where((MechanicEntity x) => (bool)x.Features.IsFirstInFight && x.IsInPlayerParty).Concat(entities.Where((MechanicEntity x) => (bool)x.Features.IsFirstInFight && !x.IsInPlayerParty));
		}
		return EtudeBracketForceInitiativeOrder.GetOrderOptional();
	}

	private static IEnumerable<MechanicEntity> GetLastOrder(List<MechanicEntity> entities)
	{
		if (EtudeBracketForceInitiativeOrder.GetOrderOptional() == null)
		{
			return entities.Where((MechanicEntity x) => (bool)x.Features.IsLastInFight && x.IsInPlayerParty).Concat(entities.Where((MechanicEntity x) => (bool)x.Features.IsLastInFight && !x.IsInPlayerParty));
		}
		return EtudeBracketForceInitiativeOrder.GetOrderOptional();
	}

	private static void ApplyInitiative(MechanicEntity entity)
	{
		Initiative initiative = entity.Initiative;
		float value = (entity.Initiative.Roll = entity.GetInitiativeRollProvider().Initiative.Roll);
		initiative.Value = value;
		entity.Initiative.Order = CalculateOrder(entity);
	}

	private static void PostProcessInitiative(List<MechanicEntity> entities)
	{
		foreach (MechanicEntity entity2 in entities)
		{
			MechanicEntity entity = entity2;
			PartMultiInitiative multiInitiative = entity.GetMultiInitiative();
			if (multiInitiative == null)
			{
				continue;
			}
			IEnumerable<InitiativePlaceholderEntity> source = multiInitiative.EnsurePlaceholders();
			int num = multiInitiative.AdditionalTurnsCount + 1;
			List<float> list = (from unit in Game.Instance.TurnController.AllUnits
				where unit.IsInCombat && unit.IsEnemy(entity)
				select unit.Initiative.Value into i
				orderby i descending
				select i).ToList();
			float num2 = (float)list.Count() / (float)num;
			int num3 = 1;
			foreach (MechanicEntity item in source.Append(entity))
			{
				int num4 = Mathf.FloorToInt(num2 * (float)num3);
				if (num4 >= list.Count() - 1)
				{
					OverrideInitiative(item, list.Last() / 2f);
				}
				else
				{
					OverrideInitiative(item, (list[num4] + list[num4 + 1]) / 2f);
				}
				num3++;
			}
			void OverrideInitiative(MechanicEntity e, float value)
			{
				e.Initiative.Roll = entity.Initiative.Roll;
				e.Initiative.Value = value;
				e.Initiative.Order = CalculateOrder(e);
			}
		}
	}

	private static int CalculateOrder(MechanicEntity entity)
	{
		return Game.Instance.TurnController.AllUnits.Count((MechanicEntity i) => i != entity && Math.Abs(i.Initiative.Value - entity.Initiative.Value) < 1E-06f);
	}

	private static int OrderEntitiesByInitiativeType(MechanicEntity e1, MechanicEntity e2)
	{
		return GetInitiativeType(e1).CompareTo(GetInitiativeType(e2));
	}

	private static InitiativeType GetInitiativeType(MechanicEntity entity)
	{
		if (entity is UnitSquad)
		{
			return InitiativeType.Squad;
		}
		PartSquad squadOptional = entity.GetSquadOptional();
		if (squadOptional != null && squadOptional.IsLeader)
		{
			return InitiativeType.SquadLeader;
		}
		if (entity.GetSquadOptional() != null)
		{
			return InitiativeType.SquadMember;
		}
		if (entity.GetInitiativeRollProvider() == entity)
		{
			return InitiativeType.Independent;
		}
		if (entity.GetMultiInitiative() == null)
		{
			return InitiativeType.Other;
		}
		return InitiativeType.Multi;
	}

	private static void UpdateBuffsInitiative(MechanicEntity entity)
	{
		foreach (Buff rawFact in entity.Buffs.RawFacts)
		{
			rawFact.UpdateCombatInitiative();
		}
	}

	private static MechanicEntity GetInitiativeRollProvider(this MechanicEntity entity)
	{
		UnitSquad unitSquad = entity.GetSquadOptional()?.Squad;
		if (unitSquad != null)
		{
			return unitSquad;
		}
		MechanicEntity mechanicEntity = entity.GetSummonedMonsterOption()?.Summoner;
		if (mechanicEntity != null)
		{
			return mechanicEntity;
		}
		return entity;
	}
}
