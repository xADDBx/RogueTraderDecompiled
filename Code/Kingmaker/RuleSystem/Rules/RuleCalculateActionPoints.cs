using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateActionPoints : RulebookEvent
{
	private readonly bool m_IsTurnBased;

	public int MaxPointsBonus { get; set; }

	public int RegenBonus { get; set; }

	public bool SetUpperLimit { get; set; }

	public int UppLimitValue { get; set; }

	public int Result { get; private set; }

	public RuleCalculateActionPoints([NotNull] MechanicEntity initiator, bool isTurnBased)
		: base(initiator)
	{
		m_IsTurnBased = isTurnBased;
		UppLimitValue = int.MaxValue;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (base.Initiator is StarshipEntity starshipEntity)
		{
			int val = Math.Max((int)starshipEntity.CombatState.WarhammerInitialAPYellow + MaxPointsBonus, 0);
			Result = Math.Min(starshipEntity.CombatState.ActionPointsYellow, val);
		}
		else if (base.Initiator is UnitEntity unitEntity)
		{
			int num = Math.Max((int)unitEntity.CombatState.WarhammerInitialAPYellow + MaxPointsBonus, 0);
			int num2 = Math.Max(BlueprintWarhammerRoot.Instance.CombatRoot.BaseActionPointsRegen, unitEntity.CombatState.WarhammerInitialAPYellow);
			Result = (m_IsTurnBased ? Math.Min(unitEntity.CombatState.ActionPointsYellow + num2 + RegenBonus, num) : num);
		}
		if (SetUpperLimit)
		{
			Result = Math.Min(Result, UppLimitValue);
		}
	}
}
