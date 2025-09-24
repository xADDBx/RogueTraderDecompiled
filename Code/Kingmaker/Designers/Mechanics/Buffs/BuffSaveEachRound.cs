using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("1680ee490b9647febec59945dc3f1fdb")]
public class BuffSaveEachRound : UnitBuffComponentDelegate, ITickEachRound, IHashable
{
	public SavingThrowType SaveType;

	public void OnNewRound()
	{
		RulePerformSavingThrow obj = new RulePerformSavingThrow(base.Owner, SaveType, 0, base.Buff.Context.MaybeCaster)
		{
			Reason = base.Fact
		};
		Rulebook.Trigger(obj);
		if (obj.IsPassed)
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
