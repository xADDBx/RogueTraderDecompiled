using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartMomentum : UnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartMomentum>, IEntityPartOwner
	{
		PartMomentum Momentum { get; }
	}

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueAttributeBonusDependent Resolve => StatsContainer.GetStat<ModifiableValueAttributeBonusDependent>(StatType.Resolve);

	protected override void OnAttach()
	{
		InitializeStats();
	}

	protected override void OnPrePostLoad()
	{
		InitializeStats();
	}

	private void InitializeStats()
	{
		StatsContainer.Register<ModifiableValueAttributeBonusDependent>(StatType.Resolve);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
