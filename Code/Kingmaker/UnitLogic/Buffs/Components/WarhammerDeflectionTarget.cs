using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("7173e6a0fca449aebdcbedbdc2afcde1")]
public class WarhammerDeflectionTarget : UnitBuffComponentDelegate, IHashable
{
	public MechanicEntity Caster { get; private set; }

	protected override void OnActivateOrPostLoad()
	{
		Caster = base.Fact.MaybeContext?.MaybeCaster;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
