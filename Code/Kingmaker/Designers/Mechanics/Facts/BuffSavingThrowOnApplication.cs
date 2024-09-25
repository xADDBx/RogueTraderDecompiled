using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("0b95b4897594493996d8d38142453fd0")]
public class BuffSavingThrowOnApplication : UnitBuffComponentDelegate, IHashable
{
	public SavingThrowType SavingThrowType;

	public int DifficultyModifier;

	protected override void OnActivate()
	{
		RulePerformSavingThrow rulePerformSavingThrow = new RulePerformSavingThrow(base.Owner, SavingThrowType, DifficultyModifier);
		Rulebook.Trigger(rulePerformSavingThrow);
		if (rulePerformSavingThrow.IsPassed)
		{
			base.Buff.Remove();
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
