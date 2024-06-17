using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Groups;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class TurnDataPart : EntityPart, IHashable
{
	[JsonProperty]
	private int m_GameRound = 1;

	[JsonProperty]
	[GameStateIgnore]
	private UnitGroup[] m_Groups = Array.Empty<UnitGroup>();

	[JsonProperty]
	public readonly List<MomentumGroup> MomentumGroups = new List<MomentumGroup>();

	[JsonProperty]
	public readonly TurnOrderQueue TurnOrder = new TurnOrderQueue();

	[JsonProperty(PropertyName = "m_TbActive")]
	public bool InCombat { get; set; }

	[JsonProperty(PropertyName = "m_EndTurnRequested")]
	public bool EndTurnRequested { get; set; }

	[JsonProperty(PropertyName = "m_IsUltimateAbilityUsedThisRound")]
	public bool IsUltimateAbilityUsedThisRound { get; set; }

	[JsonProperty(PropertyName = "m_LastTurnTime")]
	public TimeSpan LastTurnTime { get; set; }

	[JsonProperty]
	public int CombatRound { get; set; }

	public int GameRound
	{
		get
		{
			return m_GameRound;
		}
		set
		{
			if (m_GameRound > value)
			{
				PFLog.System.ErrorWithReport("Wow! Current round index is less than next round index (overflow maybe)");
			}
			m_GameRound = Math.Max(0, value);
		}
	}

	protected override void OnPreSave()
	{
		m_Groups = Game.Instance.UnitGroups.Where((UnitGroup i) => i.IsInCombat).ToArray();
	}

	protected override void OnPostLoad()
	{
		UnitGroup[] groups = m_Groups;
		foreach (UnitGroup group in groups)
		{
			Game.Instance.UnitGroupsController.RestoreGroup(group);
		}
		m_Groups = Array.Empty<UnitGroup>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = InCombat;
		result.Append(ref val2);
		bool val3 = EndTurnRequested;
		result.Append(ref val3);
		bool val4 = IsUltimateAbilityUsedThisRound;
		result.Append(ref val4);
		TimeSpan val5 = LastTurnTime;
		result.Append(ref val5);
		result.Append(ref m_GameRound);
		int val6 = CombatRound;
		result.Append(ref val6);
		List<MomentumGroup> momentumGroups = MomentumGroups;
		if (momentumGroups != null)
		{
			for (int i = 0; i < momentumGroups.Count; i++)
			{
				Hash128 val7 = ClassHasher<MomentumGroup>.GetHash128(momentumGroups[i]);
				result.Append(ref val7);
			}
		}
		Hash128 val8 = ClassHasher<TurnOrderQueue>.GetHash128(TurnOrder);
		result.Append(ref val8);
		return result;
	}
}
