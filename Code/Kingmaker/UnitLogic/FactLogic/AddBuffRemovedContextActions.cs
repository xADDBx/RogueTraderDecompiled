using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("c93350feff15464a95621a9ff2eb4a69")]
public class AddBuffRemovedContextActions : UnitFactComponentDelegate, IBuffRemoved, IHashable
{
	public ActionList Actions;

	public void OnRemoved()
	{
		base.Fact.RunActionInContext(Actions);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
