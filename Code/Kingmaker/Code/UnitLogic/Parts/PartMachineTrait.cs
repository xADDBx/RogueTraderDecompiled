using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.Parts;

public class PartMachineTrait : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartMachineTrait>, IEntityPartOwner
	{
		[CanBeNull]
		PartMachineTrait MaybeMachineTrait { get; }
	}

	private int m_RetainCount;

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValue MachineTrait => StatsContainer.GetStat(StatType.MachineTrait);

	public static int GetBaseStatValue(MechanicEntity unit)
	{
		return unit.GetStatBaseValue(StatType.MachineTrait).Value;
	}

	public void Retain()
	{
		m_RetainCount++;
		PFLog.History.Party.Log($"PartMachineTrait.Retain: {base.Owner}, count {m_RetainCount}");
	}

	public void Release()
	{
		m_RetainCount--;
		PFLog.History.Party.Log($"PartMachineTrait.Release: {base.Owner}, count {m_RetainCount}");
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
		StatsContainer.Register(StatType.MachineTrait);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
