using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStatsContainer : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStatsContainer>, IEntityPartOwner
	{
		PartStatsContainer Stats { get; }
	}

	[JsonProperty]
	public StatsContainer Container { get; private set; }

	public Dictionary<StatType, StatType> OverridenBaseStat { get; } = new Dictionary<StatType, StatType>();


	public Dictionary<StatType, StatType> OverridenStats { get; } = new Dictionary<StatType, StatType>();


	public IEnumerable<ModifiableValue> AllStats => Container.AllStats;

	protected override void OnAttach()
	{
		Container = new StatsContainer(base.Owner);
	}

	protected override void OnPrePostLoad()
	{
		Container.PrePostLoad(base.Owner);
	}

	protected override void OnPostLoad()
	{
		Container.PostLoad();
	}

	public void CleanupModifiers()
	{
		Container.CleanupModifiers();
	}

	public void AddClassSkill(StatType stat)
	{
		Container.AddClassSkill(stat);
	}

	[CanBeNull]
	public TModifiableValue GetStatOptional<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStatOptional<TModifiableValue>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStatOptional(StatType type)
	{
		return Container.GetStatOptional(type);
	}

	[CanBeNull]
	public ModifiableValueAttributeStat GetAttributeOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueAttributeStat>(type);
	}

	[CanBeNull]
	public ModifiableValueSkill GetSkillOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueSkill>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStat(StatType type, bool canBeNull = false)
	{
		return Container.GetStat(type, canBeNull);
	}

	[NotNull]
	public TModifiableValue GetStat<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStat<TModifiableValue>(type);
	}

	[NotNull]
	public ModifiableValueAttributeStat GetAttribute(StatType type)
	{
		return Container.GetAttribute(type);
	}

	[NotNull]
	public ModifiableValueSkill GetSkill(StatType type)
	{
		return Container.GetSkill(type);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<StatsContainer>.GetHash128(Container);
		result.Append(ref val2);
		return result;
	}
}
