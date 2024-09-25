using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Inspect;

public class InspectUnitsManager : IHashable
{
	public class UnitInfo : IHashable
	{
		public const int MaxKnownPartsCount = 4;

		[JsonProperty]
		public readonly BlueprintUnit Blueprint;

		[JsonProperty]
		private UnitInfoPart m_Parts;

		[JsonProperty]
		private int m_CheckValue;

		[JsonProperty]
		public bool HasUnviewedChange { get; private set; }

		public int DC => 10 + (int)Math.Floor((float)Blueprint.CR * 1.25f);

		public int SuccessMargin => m_CheckValue - DC;

		public bool Success => SuccessMargin >= 0;

		public int KnownPartsCount
		{
			get
			{
				if (!Success)
				{
					return 0;
				}
				return Math.Min(4, 1 + SuccessMargin / 5);
			}
		}

		public bool IsAllPartsUnlocked => m_Parts == UnitInfoPart.All;

		public bool IsNothingUnlocked => m_Parts == UnitInfoPart.None;

		public int CurrentKnownPartsCount => (IsUnlocked(UnitInfoPart.Base) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Defence) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Offence) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Abilities) ? 1 : 0);

		[JsonConstructor]
		public UnitInfo(BlueprintUnit blueprint)
		{
			Blueprint = blueprint;
		}

		public bool IsUnlocked(UnitInfoPart part)
		{
			return (m_Parts & part) != 0;
		}

		public void SetCheck(int check, BaseUnitEntity unit)
		{
			if (!IsAllPartsUnlocked)
			{
				int knownPartsCount = KnownPartsCount;
				m_CheckValue = Math.Max(m_CheckValue, check);
				if (knownPartsCount < KnownPartsCount)
				{
					HasUnviewedChange = true;
					TryUnlock(UnitInfoPart.Base, unit);
					TryUnlock(UnitInfoPart.Defence, unit);
					TryUnlock(UnitInfoPart.Offence, unit);
					TryUnlock(UnitInfoPart.Abilities, unit);
				}
			}
		}

		public void TryUnlock(UnitInfoPart part, BaseUnitEntity unit)
		{
			if (CurrentKnownPartsCount < KnownPartsCount && !IsUnlocked(part))
			{
				m_Parts |= part;
			}
		}

		public void MarkViewed()
		{
			HasUnviewedChange = false;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
			result.Append(ref val);
			result.Append(ref m_Parts);
			result.Append(ref m_CheckValue);
			bool val2 = HasUnviewedChange;
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<UnitInfo> m_UnitInfos = new List<UnitInfo>();

	public IEnumerable<UnitInfo> UnitInfos => m_UnitInfos;

	[CanBeNull]
	public UnitInfo GetInfo(BlueprintUnit unit)
	{
		return m_UnitInfos.FirstItem((UnitInfo i) => i.Blueprint == unit);
	}

	[CanBeNull]
	public UnitInfo GetInfo(BaseUnitEntity unit)
	{
		return GetInfo(unit.BlueprintForInspection);
	}

	public static UnitInfo GetInfoForce(BaseUnitEntity unit)
	{
		return new UnitInfo(unit.BlueprintForInspection);
	}

	public bool TryMakeKnowledgeCheck(BaseUnitEntity unit)
	{
		bool num = InspectUnitsHelper.IsInspectAllow(unit);
		bool result = false;
		if (!num)
		{
			return result;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		if (info.KnownPartsCount == 4)
		{
			return result;
		}
		int dC = info.DC;
		StatType statType = StatType.SkillLoreXenos;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!item.LifeState.IsConscious)
			{
				continue;
			}
			if ((item.Stats.GetStat<ModifiableValueSkill>(statType)?.BaseValue ?? 0) > 0)
			{
				RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(item, statType, dC)
				{
					IgnoreDifficultyBonusToDC = true
				});
				if (rulePerformSkillCheck.ResultIsSuccess)
				{
					result = true;
				}
				info.SetCheck(rulePerformSkillCheck.RollResult, item);
			}
			if (info.IsAllPartsUnlocked)
			{
				break;
			}
		}
		EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
		{
			h.HandleKnowledgeUpdated(info);
		});
		return result;
	}

	public bool TryMakeKnowledgeCheck(BaseUnitEntity unit, BaseUnitEntity inspector)
	{
		bool num = InspectUnitsHelper.IsInspectAllow(unit);
		bool result = false;
		if (!num)
		{
			return result;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		int dC = info.DC;
		StatType statType = StatType.SkillLoreXenos;
		if (!inspector.LifeState.IsConscious)
		{
			return result;
		}
		if ((inspector.Stats.GetStat<ModifiableValueSkill>(statType)?.BaseValue ?? 0) > 0)
		{
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(inspector, statType, dC)
			{
				IgnoreDifficultyBonusToDC = true
			});
			if (rulePerformSkillCheck.ResultIsSuccess)
			{
				result = true;
			}
			info.SetCheck(rulePerformSkillCheck.RollResult, inspector);
		}
		EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
		{
			h.HandleKnowledgeUpdated(info);
		});
		return result;
	}

	public void ForceRevealUnitInfo(BaseUnitEntity unit)
	{
		if (!InspectUnitsHelper.IsInspectAllow(unit))
		{
			return;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		if (info.KnownPartsCount != 4)
		{
			EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
			{
				h.HandleKnowledgeUpdated(info);
			});
			info.SetCheck(100, Game.Instance.Player.MainCharacterEntity);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<UnitInfo> unitInfos = m_UnitInfos;
		if (unitInfos != null)
		{
			for (int i = 0; i < unitInfos.Count; i++)
			{
				Hash128 val = ClassHasher<UnitInfo>.GetHash128(unitInfos[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
