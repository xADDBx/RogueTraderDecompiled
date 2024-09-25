using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("048052d407734554a3b4a356b34c13d6")]
public class SetChargeWeapon : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, SetChargeWeapon>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			base.Owner.GetOrCreate<UnitPartChargeWeapon>().Set(base.SourceBlueprintComponent.Weapon);
		}

		protected override void OnDeactivate()
		{
			base.Owner.Remove<UnitPartChargeWeapon>();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private BlueprintItemWeaponReference m_Weapon;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
