using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AI.Scenarios;

public abstract class AiScenario : IHashable
{
	[JsonProperty]
	protected readonly BaseUnitEntity Owner;

	[JsonProperty]
	private readonly int IdleRoundsCountLimit;

	[JsonProperty]
	public bool IsComplete { get; private set; }

	[JsonProperty]
	public ConditionsChecker CustomConditions { get; set; }

	public AiScenario(BaseUnitEntity owner, int idleRoundsCountLimit)
	{
		Owner = owner;
		IdleRoundsCountLimit = idleRoundsCountLimit;
	}

	[JsonConstructor]
	protected AiScenario()
	{
	}

	public void Complete()
	{
		IsComplete = true;
		EventBus.RaiseEvent((IBaseUnitEntity)Owner, (Action<IUnitAiScenarioHandler>)delegate(IUnitAiScenarioHandler h)
		{
			h.HandleScenarioDeactivated(this);
		}, isCheckRuntime: true);
	}

	public virtual bool ShouldComplete()
	{
		if (IdleRoundsCountLimit != 0 && Owner.Brain.IdleRoundsCount >= IdleRoundsCountLimit)
		{
			return true;
		}
		if (CustomConditions != null && CustomConditions.HasConditions)
		{
			return CustomConditions.Check();
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<BaseUnitEntity>.GetHash128(Owner);
		result.Append(ref val);
		int val2 = IdleRoundsCountLimit;
		result.Append(ref val2);
		bool val3 = IsComplete;
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<ConditionsChecker>.GetHash128(CustomConditions);
		result.Append(ref val4);
		return result;
	}
}
