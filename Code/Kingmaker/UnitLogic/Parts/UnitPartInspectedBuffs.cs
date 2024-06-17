using System.Collections.Generic;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Inspect;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartInspectedBuffs : BaseUnitPart, IHashable
{
	public struct CasterInspectionInfo : IHashable
	{
		[JsonProperty]
		public EntityRef<MechanicEntity> Caster;

		[JsonProperty]
		public bool CheckPassed;

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<MechanicEntity> obj = Caster;
			Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref CheckPassed);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<CasterInspectionInfo> m_InspectedCasters = new List<CasterInspectionInfo>();

	private bool MakeCheck(BaseUnitEntity caster)
	{
		InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(caster.BlueprintForInspection);
		if (info == null)
		{
			return false;
		}
		int dC = info.DC;
		StatType statType = StatType.SkillTechUse;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.LifeState.IsConscious && (item.Stats.GetStat<ModifiableValueSkill>(statType)?.BaseValue ?? 0) > 0 && GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(item, statType, dC)
			{
				IgnoreDifficultyBonusToDC = true
			}, null, allowPartyCheckInCamp: false).RollResult >= dC)
			{
				return true;
			}
		}
		return false;
	}

	public List<Buff> GetBuffs(UnitInspectInfoByPart inspectInfo)
	{
		List<Buff> list;
		if (inspectInfo != null)
		{
			inspectInfo.ActiveBuffsPart = new UnitInspectInfoByPart.ActiveBuffsPartData();
			list = inspectInfo.ActiveBuffsPart.ActiveBuffs;
		}
		else
		{
			list = new List<Buff>();
		}
		if (!InspectUnitsHelper.IsInspectAllow(base.Owner))
		{
			return null;
		}
		Dictionary<MechanicEntity, List<Buff>> dictionary = new Dictionary<MechanicEntity, List<Buff>>();
		foreach (Buff buff in base.Owner.Buffs)
		{
			if (buff.Name.Empty() || buff.Blueprint.IsHiddenInUI)
			{
				continue;
			}
			MechanicEntity maybeCaster = buff.Context.MaybeCaster;
			if (maybeCaster == null)
			{
				list.Add(buff);
				continue;
			}
			if (!dictionary.ContainsKey(maybeCaster))
			{
				dictionary.Add(maybeCaster, new List<Buff>());
			}
			dictionary[maybeCaster].Add(buff);
		}
		foreach (KeyValuePair<MechanicEntity, List<Buff>> buffList in dictionary)
		{
			if (!(buffList.Key is BaseUnitEntity baseUnitEntity))
			{
				continue;
			}
			bool flag = Game.Instance.Player.AllCharacters.Contains(baseUnitEntity);
			if (!flag)
			{
				if (m_InspectedCasters.FindIndex((CasterInspectionInfo info) => info.Caster == buffList.Key) == -1)
				{
					flag = true;
					m_InspectedCasters.Add(new CasterInspectionInfo
					{
						Caster = baseUnitEntity,
						CheckPassed = flag
					});
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.AddRange(buffList.Value.ToArray());
			}
		}
		return list;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<CasterInspectionInfo> inspectedCasters = m_InspectedCasters;
		if (inspectedCasters != null)
		{
			for (int i = 0; i < inspectedCasters.Count; i++)
			{
				CasterInspectionInfo obj = inspectedCasters[i];
				Hash128 val2 = StructHasher<CasterInspectionInfo>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
