using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Buffs/AddEffect/Immortality")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("2f139fc00ae2f194c9aae4a2cf82d4ae")]
public class AddImmortality : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, AddImmortality>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			base.Owner.Features.Immortality.Retain();
		}

		protected override void OnDeactivate()
		{
			base.Owner.Features.Immortality.Release();
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
