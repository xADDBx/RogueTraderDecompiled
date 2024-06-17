using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartPsyker : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartPsyker>, IEntityPartOwner
	{
		[CanBeNull]
		PartPsyker MaybePsyker { get; }
	}

	private int m_RetainCount;

	public int AdditionChanceOnPsychicPhenomena;

	public int AdditionChanceOnPerilsOfWarp;

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValue PsyRating => StatsContainer.GetStat(StatType.PsyRating);

	public void Retain()
	{
		m_RetainCount++;
		PFLog.History.Party.Log($"PartPsyker.Retain: {base.Owner}, count {m_RetainCount}");
	}

	public void Release()
	{
		m_RetainCount--;
		PFLog.History.Party.Log($"PartPsyker.Release: {base.Owner}, count {m_RetainCount}");
		if (m_RetainCount < 1)
		{
			RemoveSelf();
		}
	}

	protected override void OnAttach()
	{
		Initialize();
	}

	private void Initialize()
	{
		StatsContainer.Register(StatType.PsyRating);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
