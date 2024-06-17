using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Passive/Mark passive")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1038bb1095561e449b7d2df24d92441b")]
public class MarkPassive : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, MarkPassive>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			base.Owner.Passive.Retain();
		}

		protected override void OnDeactivate()
		{
			base.Owner.Passive.Release();
		}

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
