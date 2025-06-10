using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e0669db0f27a42f89e89b784e47eff58")]
public class TransientPartyMemberFlag : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, TransientPartyMemberFlag>, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
