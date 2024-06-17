using System;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStarshipMorale : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipMorale>, IEntityPartOwner
	{
		PartStarshipMorale Morale { get; }
	}

	private int m_MoraleDamage;

	public int MoraleDamage
	{
		get
		{
			return m_MoraleDamage;
		}
		set
		{
			m_MoraleDamage = Math.Max(0, value);
		}
	}

	public int MaxMorale => StatsContainer.GetStat(StatType.Morale);

	public int MoraleLeft => MaxMorale - MoraleDamage;

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
