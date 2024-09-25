using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Allows KO condition")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("addae4953430725479cc27bae68ad849")]
public class AllowDyingCondition : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, AllowDyingCondition>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			base.Owner.Features.AllowDyingCondition.Retain();
		}

		protected override void OnDeactivate()
		{
			base.Owner.Features.AllowDyingCondition.Release();
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
