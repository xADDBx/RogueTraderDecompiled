using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("bb145d4e69427d641a4d60ab345f05da")]
public class WarhammerBoardingBuff : UnitBuffComponentDelegate, ITickEachRound, IHashable
{
	public int BoardingRating;

	public int MoraleDamagePerRound;

	public void OnNewRound()
	{
		if (base.Buff.Context.MainTarget.Entity is StarshipEntity starshipEntity)
		{
			int num = base.Buff.RoundNumber * (int)starshipEntity.Starship.MilitaryRating;
			int num2 = Math.Max(0, BoardingRating - num);
			PFLog.Default.Log($"Boarding remainder: {num2}, military rating: {(int)starshipEntity.Starship.MilitaryRating} ({1 + (num2 - 1) / (int)starshipEntity.Starship.MilitaryRating} rounds remains)");
			if (num2 <= 0)
			{
				base.Buff.Remove();
			}
			else
			{
				Rulebook.Trigger(new RuleDealStarshipMoraleDamage(base.Buff.Context.MaybeCaster, starshipEntity, MoraleDamagePerRound));
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
