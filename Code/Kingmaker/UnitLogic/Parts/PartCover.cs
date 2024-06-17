using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartCover : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartCover>, IEntityPartOwner
	{
		PartCover Cover { get; }
	}

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueHitPoints HitPoints => StatsContainer.GetStat<ModifiableValueHitPoints>(StatType.HitPoints);

	public ModifiableValueAttributeStat Toughness => StatsContainer.GetAttribute(StatType.WarhammerToughness);

	public ModifiableValue DamageDeflection => StatsContainer.GetAttribute(StatType.DamageDeflection);

	public ModifiableValue DamageAbsorption => StatsContainer.GetAttribute(StatType.DamageAbsorption);

	protected override void OnAttach()
	{
		StatsContainer.RegisterAttribute(StatType.WarhammerToughness);
		StatsContainer.Register<ModifiableValueHitPoints>(StatType.HitPoints);
		StatsContainer.Register(StatType.DamageAbsorption);
		StatsContainer.Register(StatType.DamageDeflection);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
