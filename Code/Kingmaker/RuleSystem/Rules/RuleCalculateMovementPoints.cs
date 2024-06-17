using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateMovementPoints : RulebookEvent
{
	public int Result { get; set; }

	public int Bonus { get; set; }

	public float Modifier { get; set; }

	public bool SetUpperLimit { get; set; }

	public int UppLimitValue { get; set; }

	public RuleCalculateMovementPoints([NotNull] MechanicEntity initiator)
		: base(initiator)
	{
		Modifier = 1f;
		UppLimitValue = int.MaxValue;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ModifiableValue modifiableValue = ((MechanicEntity)base.Initiator).GetCombatStateOptional()?.WarhammerInitialAPBlue;
		int num = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
		Result = Mathf.RoundToInt((float)(num + Bonus) * Modifier);
		if (base.Initiator is StarshipEntity starshipEntity)
		{
			Result = starshipEntity.Navigation.CurrentSpeed;
		}
		else
		{
			int num2 = ((MechanicEntity)base.Initiator).GetOrCreate<UnitPartJumpAsideDodge>()?.SpentMovePoints ?? 0;
			Result -= num2;
		}
		if (SetUpperLimit)
		{
			Result = Math.Min(Result, UppLimitValue);
		}
		Result = Math.Max(Result, 0);
	}
}
