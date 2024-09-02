using System;
using System.Collections.Generic;
using Code.Enums;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("f13ba6e499da486e9ec3ddc458c6c110")]
public class DOTLogic : UnitBuffComponentDelegate, ITickEachRound, IHashable
{
	public class Settings : ContextData<Settings>
	{
		public int Damage { get; private set; }

		public SavingThrowType? SaveOverride { get; private set; }

		public int? DifficultyOverride { get; private set; }

		public int? PenetrationOverride { get; private set; }

		public Settings Setup(int damage, SavingThrowType? saveType, int? difficulty, int? penetration)
		{
			Damage = damage;
			SaveOverride = saveType;
			DifficultyOverride = difficulty;
			PenetrationOverride = penetration;
			return this;
		}

		protected override void Reset()
		{
			Damage = 0;
			SaveOverride = null;
			DifficultyOverride = null;
			PenetrationOverride = null;
		}
	}

	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int Damage;

		[JsonProperty]
		public SavingThrowType? SaveTypeOverride;

		[JsonProperty]
		public int? DifficultyOverride;

		[JsonProperty]
		public int? PenetrationOverride;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref Damage);
			if (SaveTypeOverride.HasValue)
			{
				SavingThrowType val2 = SaveTypeOverride.Value;
				result.Append(ref val2);
			}
			if (DifficultyOverride.HasValue)
			{
				int val3 = DifficultyOverride.Value;
				result.Append(ref val3);
			}
			if (PenetrationOverride.HasValue)
			{
				int val4 = PenetrationOverride.Value;
				result.Append(ref val4);
			}
			return result;
		}
	}

	private class PartDOTDirector : MechanicEntityPart, IHashable
	{
		private readonly struct Entry
		{
			public readonly Buff Buff;

			public readonly DOTLogic Logic;

			public int Damage => GetData().Damage;

			public SavingThrowType SaveType => GetData()?.SaveTypeOverride ?? Logic.SaveType;

			public int Difficulty => GetData().DifficultyOverride ?? Logic.Difficulty;

			public int Penetration => (GetData()?.PenetrationOverride).GetValueOrDefault();

			public Entry(Buff buff, DOTLogic logic)
			{
				this = default(Entry);
				Buff = buff;
				Logic = logic;
			}

			public Data GetData()
			{
				return Buff.RequestSavableData<Data>(Logic);
			}
		}

		private readonly Dictionary<DOT, List<Entry>> m_DOTs = new Dictionary<DOT, List<Entry>>();

		public void Register(Buff buff, DOTLogic logic)
		{
			if (buff != null)
			{
				List<Entry> list = m_DOTs.Get(logic.Type);
				if (list == null)
				{
					m_DOTs.Add(logic.Type, list = new List<Entry>());
				}
				list.Add(new Entry(buff, logic));
				UpdateList(list);
			}
		}

		public void Unregister(Buff buff, DOTLogic logic)
		{
			if (buff != null)
			{
				List<Entry> list = m_DOTs.Get(logic.Type);
				list.RemoveAll((Entry i) => i.Buff == buff && i.Logic == logic);
				if (list.Empty())
				{
					m_DOTs.Remove(logic.Type);
				}
				else
				{
					UpdateList(list);
				}
				if (m_DOTs.Empty())
				{
					RemoveSelf();
				}
			}
		}

		public void OnNewRound(Buff buff, DOTLogic logic, bool onlyDamage = false, MechanicEntity targetOverride = null)
		{
			if (buff == null)
			{
				return;
			}
			MechanicEntity mechanicEntity = targetOverride ?? base.Owner;
			List<Entry> list = m_DOTs.Get(logic.Type);
			if (list.Empty())
			{
				return;
			}
			Entry entry = list[list.Count - 1];
			if (entry.Buff != buff || entry.Logic != logic)
			{
				return;
			}
			int damage = entry.Damage;
			int penetration = entry.Penetration;
			if (damage > 0)
			{
				MechanicEntity mechanicEntity2 = buff.Context.MaybeCaster ?? mechanicEntity;
				DamageData baseDamageOverride = logic.DamageType.CreateDamage(damage);
				CalculateDamageParams calculateDamageParams = new CalculateDamageParams(mechanicEntity2, mechanicEntity, buff.Context.SourceAbilityContext?.Ability, null, baseDamageOverride, penetration, mechanicEntity2.DistanceToInCells(mechanicEntity));
				calculateDamageParams.Reason = buff;
				RuleCalculateDamage ruleCalculateDamage = calculateDamageParams.Trigger();
				if ((bool)mechanicEntity.Features.HealInsteadOfDamageForDOTs)
				{
					Rulebook.Trigger(new RuleHealDamage(mechanicEntity, mechanicEntity, ruleCalculateDamage.ResultDamage.BaseRolledValue));
				}
				else
				{
					Rulebook.Trigger(new RuleDealDamage(mechanicEntity2, mechanicEntity, ruleCalculateDamage.ResultDamage)
					{
						Reason = buff
					});
				}
			}
			if (onlyDamage || ((bool)base.Owner.Features.ShapeFlames && ShapeFlames.SaveIgnoreList.Contains(logic.Type)))
			{
				return;
			}
			int num = int.MinValue;
			foreach (Entry item in list)
			{
				int difficulty = item.Difficulty;
				if (difficulty > num)
				{
					num = difficulty;
				}
			}
			SavingThrowType saveType = entry.SaveType;
			if (saveType == SavingThrowType.Unknown)
			{
				return;
			}
			RulePerformSavingThrow obj = new RulePerformSavingThrow(base.Owner, saveType, num)
			{
				Reason = buff
			};
			Rulebook.Trigger(obj);
			if (!obj.IsPassed)
			{
				return;
			}
			foreach (Entry item2 in list)
			{
				item2.Buff.MarkExpired();
			}
		}

		private static void UpdateList(List<Entry> list)
		{
			list.Sort((Entry i1, Entry i2) => i1.Damage.CompareTo(i2.Damage));
		}

		public DamageData GetDamageDataOfType(DOT type)
		{
			if (TryCreateDamageDataOfType(type, out var damageData, out var _))
			{
				return damageData;
			}
			return null;
		}

		public int GetCurrentDamageOfType(DOT type)
		{
			if (TryCreateDamageDataOfType(type, out var damageData, out var _))
			{
				return damageData.AverageValue;
			}
			return 0;
		}

		public int GetDamageOfTypeInstancesCount(DOT type)
		{
			List<Entry> list = m_DOTs.Get(type);
			if (list.Empty())
			{
				return 0;
			}
			int num = 0;
			foreach (Entry item in list)
			{
				if (item.Logic != null && item.Buff != null && item.Damage > 0)
				{
					num++;
				}
			}
			return num;
		}

		public int GetBasicDamageOfType(DOT type)
		{
			List<Entry> list = m_DOTs.Get(type);
			if (list.Empty())
			{
				return 0;
			}
			return list[list.Count - 1].Damage;
		}

		public bool TryDealDamageByDOTImmediately(DOT type, MechanicEntity target)
		{
			if (!TryCreateDamageDataOfType(type, out var damageData, out var dotEntry))
			{
				return false;
			}
			if ((bool)target.Features.HealInsteadOfDamageForDOTs)
			{
				Rulebook.Trigger(new RuleHealDamage(dotEntry.Buff.Context.MaybeCaster ?? base.Owner, target, damageData.AverageValue)
				{
					Reason = dotEntry.Buff
				});
			}
			else
			{
				Rulebook.Trigger(new RuleDealDamage(dotEntry.Buff.Context.MaybeCaster ?? base.Owner, target, damageData)
				{
					Reason = dotEntry.Buff
				});
			}
			return true;
		}

		private bool TryCreateDamageDataOfType(DOT type, out DamageData damageData, out Entry dotEntry)
		{
			damageData = null;
			dotEntry = default(Entry);
			List<Entry> list = m_DOTs.Get(type);
			if (list.Empty())
			{
				return false;
			}
			int index = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if (TryCreateDamageDataForEntry(list[i], out var data) && (damageData == null || data.AverageValue > (damageData?.AverageValue ?? 0)))
				{
					damageData = data;
					index = i;
				}
			}
			if (damageData == null)
			{
				return false;
			}
			dotEntry = list[index];
			return true;
		}

		private bool TryCreateDamageDataForEntry(Entry dotEffect, out DamageData data)
		{
			data = null;
			if (dotEffect.Buff == null || dotEffect.Logic == null)
			{
				return false;
			}
			int damage = dotEffect.Damage;
			int penetration = dotEffect.Penetration;
			if (damage <= 0)
			{
				return false;
			}
			MechanicEntity mechanicEntity = dotEffect.Buff.Context.MaybeCaster ?? base.Owner;
			DamageData baseDamageOverride = dotEffect.Logic.DamageType.CreateDamage(damage);
			CalculateDamageParams calculateDamageParams = new CalculateDamageParams(mechanicEntity, base.Owner, dotEffect.Buff.Context.SourceAbilityContext?.Ability, null, baseDamageOverride, penetration, mechanicEntity.DistanceToInCells(base.Owner));
			calculateDamageParams.Reason = dotEffect.Buff;
			RuleCalculateDamage ruleCalculateDamage = calculateDamageParams.Trigger();
			data = ruleCalculateDamage.ResultDamage;
			return true;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public DOT Type;

	public DamageType DamageType;

	public SavingThrowType SaveType;

	public int Difficulty;

	protected override void OnInitialize()
	{
		Data data = RequestSavableData<Data>();
		data.Damage = ContextData<Settings>.Current?.Damage ?? 0;
		data.SaveTypeOverride = ContextData<Settings>.Current?.SaveOverride;
		data.DifficultyOverride = ContextData<Settings>.Current?.DifficultyOverride;
		data.PenetrationOverride = ContextData<Settings>.Current?.PenetrationOverride;
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartDOTDirector>().Register(base.Fact as Buff, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartDOTDirector>()?.Unregister(base.Fact as Buff, this);
	}

	void ITickEachRound.OnNewRound()
	{
		base.Owner.GetOptional<PartDOTDirector>()?.OnNewRound(base.Fact as Buff, this);
	}

	public static void DealDamageByDOTImmediately(MechanicEntity entity, MechanicEntity target, DOT type)
	{
		entity.GetOptional<PartDOTDirector>()?.TryDealDamageByDOTImmediately(type, target);
	}

	public static DamageData GetDamageDataOfType(MechanicEntity entity, DOT type)
	{
		return entity.GetOptional<PartDOTDirector>()?.GetDamageDataOfType(type);
	}

	public static int GetCurrentDamageOfType(MechanicEntity entity, DOT type)
	{
		return entity.GetOptional<PartDOTDirector>()?.GetCurrentDamageOfType(type) ?? 0;
	}

	public static int GetDamageOfTypeInstancesCount(MechanicEntity entity, DOT type)
	{
		return entity.GetOptional<PartDOTDirector>()?.GetDamageOfTypeInstancesCount(type) ?? 0;
	}

	public static int GetBasicDamageOfType(MechanicEntity entity, DOT type)
	{
		return entity.GetOptional<PartDOTDirector>()?.GetBasicDamageOfType(type) ?? 0;
	}

	public static void Tick(Buff buff, DOTLogic logic, bool onlyDamage = false, MechanicEntity target = null)
	{
		buff.Owner.GetOptional<PartDOTDirector>()?.OnNewRound(buff, logic, onlyDamage, target);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
